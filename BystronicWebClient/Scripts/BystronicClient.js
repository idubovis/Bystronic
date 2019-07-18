var app = angular.module("BystronicClientModule", ['ngCookies', 'connection', 'ui.toggle']);
app.controller("MainController", ['$scope', '$http', '$cookies', '$timeout', '$interval', 'ConnectionService', function ($scope, $http, $cookies, $timeout, $interval, Connection) {
    $scope.init = function () {
        FastClick.attach(document.body);

        if (!sessionStorage || !sessionStorage.length) {
            $scope.logOut(); //  do not let client access the main page before login        
            return;
        }

        $scope.version = getVersion();
        $scope.MajorVersion = getVersionInfo().MajorVersion;
        $scope.IsCanadianVersion = getVersionInfo().Country === 'CA';
        $scope.IsDemoVersion = isDemoVersion();

        $scope.saleTypes = ['Dealer Sale', 'Direct Sale'];

        if ($scope.IsCanadianVersion) {
            $scope.regions = ['Canada'];
            $scope.roles = { dealer: 'Dealer', dse: 'DSE', pm: 'PM', approver: 'Approver', administrator: 'Administrator' };
            $scope.salesmenRoles = [$scope.roles.dealer, $scope.roles.dse];
        }
        else {
            $scope.regions = ['Central', 'Midwest', 'West', 'Southwest', 'East', 'North', 'South', 'Canada'];
            $scope.roles = { dealer: 'Dealer', dse: 'DSE', rsm: 'RSM', rsmdse: 'RSMDSE', pm: 'PM', approver: 'Approver', administrator: 'Administrator' };
            $scope.salesmenRoles = [$scope.roles.dealer, $scope.roles.dse, $scope.roles.rsmdse, $scope.roles.rsm];
        }

        $scope.statuses = { pending: 'pending', approved: 'approved', released: 'released', paid: 'paid' };

        $scope.firstAvailableYear = 2018;
        $scope.lastAvailableYear = 2030;
        $scope.years = [];
        for (i = $scope.firstAvailableYear; i <= $scope.lastAvailableYear; i++) $scope.years.push(i);
        $scope.selectedYear = $scope.year = new Date().getFullYear();
        $scope.availableYears = [];
        for (i = $scope.firstAvailableYear; i <= $scope.year; i++) $scope.availableYears.push(i);

        $scope.clearOrderFilters();

        $scope.service_url = $cookies.get("bystronic_service_url");

        var settings = $cookies.get("bystronic_client_settings");
        if (settings)
            $scope.settings = JSON.parse(settings);
        else
            $scope.settings = { IdleTimeout: 5 };

        $scope.user = JSON.parse(sessionStorage.user);
        $scope.permissions = JSON.parse(sessionStorage.permissions);

        Connection.init($scope.user);

        $scope.views = JSON.parse(sessionStorage.views);
        $scope.currentView = sessionStorage.currentView;
        if (!$scope.currentView)
            sessionStorage.currentView = $scope.currentView = 'Default';
        if (viewExists($scope.currentView)) {
            $scope.columns = getViewByName($scope.currentView).Columns.split(',');
        }
        else if ($scope.currentView === 'Default') { // user has no views yet
            $scope.columns = $scope.getAllAvailableColumns();
            let columnsAsString = $scope.columns.join(',');
            Connection.saveColumns($scope.user, $scope.currentView, $scope.currentView, columnsAsString);
        }
        else { // user does not have the requested view -> switch to 'Default'
            sessionStorage.currentView = $scope.currentView = 'Default';
            $scope.columns = getDefaultView().Columns.split(',');
        }

        $scope.orderGrid = new OrderGrid($scope, '#order_grid', onOrderDoubleClicked);
        jQuery("#order_grid").jqGrid('bindKeys');

        $scope.orderInputDataGridWithoutTooling = new OrderInputDataGrid($scope, '#order_input_data_grid', false, $scope.editOrderProducts);
        jQuery("#order_input_data_grid").jqGrid('bindKeys');

        $scope.orderInputDataGridWithTooling = new OrderInputDataGrid($scope, '#order_input_data_grid_with_tooling', true, $scope.editOrderProducts);
        jQuery("#order_input_data_grid_with_tooling").jqGrid('bindKeys');

        $scope.orderOutputDataGrid = new OrderOutputDataGrid($scope, '#order_output_data_grid');
        jQuery("#order_output_data_grid").jqGrid('bindKeys');

        $scope.orderFootnoteDataGrid = new OrderFootnoteDataGrid($scope, '#order_footnote_data_grid');
        jQuery("#order_footnote_data_grid").jqGrid('bindKeys');

        $scope.productGridWithoutTooling = new ProductGrid($scope, '#product_grid', false, onProductSelected);
        jQuery("#product_grid").jqGrid('bindKeys');

        $scope.productGridWithTooling = new ProductGrid($scope, '#product_grid_with_tooling', true, onProductSelected);
        jQuery("#product_grid_with_tooling").jqGrid('bindKeys');

        $scope.productTotalGrid = new ProductTotalGrid($scope, '#product_total_grid');
        jQuery("#product_total_grid").jqGrid('bindKeys');

        $scope.productSellPriceGrid = new ProductSellPriceGrid($scope, '#product_sellprice_grid');
        jQuery("#product_sellprice_grid").jqGrid('bindKeys');

        $scope.customerListGrid = new CustomerListGrid($scope, '#customerlist_grid');
        jQuery("#customerlist_grid").jqGrid('bindKeys');

        $scope.salesmenListGrid = new SalesmenListGrid($scope, '#salesmenlist_grid');
        jQuery("#salesmenlist_grid").jqGrid('bindKeys');

        $scope.salesListGrid = new SalesListGrid($scope, '#saleslist_grid');
        jQuery("#saleslist_grid").jqGrid('bindKeys');

        $scope.productListGrid = new ProductListGrid($scope, '#productlist_grid');
        jQuery("#productlist_grid").jqGrid('bindKeys');

        $scope.columnList = new ColumnList($scope, '#column_grid');
        jQuery("#column_grid").jqGrid('bindKeys');

        hideTableHeader("#order_output_data_grid");
        hideTableHeader("#order_footnote_data_grid");
        hideTableHeader("#product_total_grid");
        hideTableHeader("#product_sellprice_grid");

        $("#product_grid").parents('div.ui-jqgrid-bdiv').css("max-height", "200px");
        $("#product_grid_with_tooling").parents('div.ui-jqgrid-bdiv').css("max-height", "200px");

        $("#product_grid").parents('div.ui-jqgrid-bdiv').css("overflow-y", "scroll");
        $("#product_grid_with_tooling").parents('div.ui-jqgrid-bdiv').css("overflow-y", "scroll");

        $("#column_grid").parents('div.ui-jqgrid-bdiv').css("overflow-y", "scroll");

        $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * .99*/);

        reloadData();

        $scope.lastAccessTime = new Date();
        $scope.interval = $interval(function () {
            if (new Date() - $scope.lastAccessTime >= $scope.settings.IdleTimeout * 60 * 1000) $scope.logOut();
        }, 15000);

        $scope.isFilterPanelExpanded = false;
    };

    $('#filterPanel').on('hide.bs.collapse', function () {
        $scope.isFilterPanelExpanded = false;
        layoutOrderGrid();
    });

    $('#filterPanel').on('shown.bs.collapse', function () {
        $scope.isFilterPanelExpanded = true;
        layoutOrderGrid();
    });

    $('body').on("click mousemove keyup", function () {
        $scope.lastAccessTime = new Date(); // wake up from idle state
    });

    $('.date').datepicker({
        allowDeselection: false,
        keyboardNavigation: true,
        forceParse: false,
        calendarWeeks: true,
        format: "mm/dd/yy",
        todayBtn: false,
        autoclose: true,
        todayHighlight: true
    });

    $('.currency').inputmask("numeric", {
        radixPoint: ".",
        groupSeparator: ",",
        digits: 2,
        autoGroup: true,
        prefix: '$', //No Space, this will truncate the first character
        rightAlign: false,
        autoUnmask: true,
        oncleared: function () { self.Value(''); }
    });

    $('.percent').inputmask("numeric", {
        radixPoint: ".",
        groupSeparator: ",",
        digits: 2,
        autoGroup: true,
        suffix: '%', //No Space, this will truncate the first character
        rightAlign: false,
        autoUnmask: true,
        oncleared: function () { self.Value(''); }
    });

    $('.dropdown-submenu a').click(function (e) {
        e.stopPropagation();
    });

    reloadData = function () {
        $scope.loaded = false;
        Connection.getData($scope.user,
            function (response) {
                //response = unzipData(response);

                if (response.Status == true) {
                    $scope.filteredSalesmen = $scope.salesmen = response.Data.Salesmen;
                    $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                    $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                    $scope.customers = response.Data.Customers;
                    $scope.tmpCustomers = _.sortBy($.extend(true, [], $scope.customers), 'Name');
                    $scope.tmpCustomers.push({ ID: 999, Name: 'Add...' });
                    $scope.products = response.Data.Products;
                    $scope.templates = response.Data.Templates;
                    $scope.productTypes = response.Data.ProductTypes;
                    $scope.orders = response.Data.Orders;

                    $.each($scope.orders, function (index, order) {
                        normalizeOrder(order);
                    });

                    layoutOrderGrid();
                    $scope.orderGrid.updateData($scope.orders, $scope.filter);
                    $scope.loaded = true;
                }
                else {
                    showError(response.Data.Error);
                    $scope.loaded = true;
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $(window).bind('resize', function () {
        $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * .99*/);
        $('#order_grid').trigger("reloadGrid");
    });

    showError = function (error) {
        if (typeof error === "string")
            $scope.error = { Message: error };
        else
            $scope.error = { Message: error.Message, Details: error.Details };
        $("#error").modal({ 'backdrop': 'static' });
        $("#error").draggable({ handle: ".modal-dialog", cursor: "move" });
        $timeout(function () { $scope.$apply(); });
    };

    layoutOrderGrid = function () {
        let gridHeight = $scope.isFilterPanelExpanded ? "45vh" : "65vh";
        $("#order_grid").parents('div.ui-jqgrid-bdiv').css("max-height", gridHeight);
    };

/////////////////////////////    O R D E R S    ////////////////////////////////////////////////////////////////////////////////

    onOrderDoubleClicked = function (orderID) {
        $scope.order = findOrderFor(orderID);
        Connection.refreshOrderYtdSale($scope.user, $scope.order.ID,
            function (response) {
                if (response.Status == true) {
                    $scope.order = normalizeOrder(response.Data.Order);
                    $scope.editOrder();
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    };

    $scope.newOrder = function (templateName) {
        var templateID = _.find($scope.templates, function (t) { return t.Name == templateName; }).ID;
        Connection.createOrder($scope.user, templateID,
            function (response) {
                if (response.Status == true) {
                    $scope.order = normalizeOrder(response.Data.Order);
                    $scope.editOrder(true);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.editOrder = function (isNewOrder) {
        $scope.isNewOrder = isNewOrder === true;
        $scope.tmpOrder = $.extend(true, {}, $scope.order); // deep copy

        $scope.orderInputDataGrid = ($scope.orderHasTooling() ? $scope.orderInputDataGridWithTooling : $scope.orderInputDataGridWithoutTooling);
        $scope.orderInputDataGrid.updateData($scope.tmpOrder);
        $scope.orderOutputDataGrid.updateData($scope.tmpOrder);
        $scope.orderFootnoteDataGrid.updateData($scope.tmpOrder);

        $scope.tmpOrder.OrderDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.OrderDate);
        $('#orderdate').datepicker('setDate', $scope.tmpOrder.OrderDateString);
        $scope.tmpOrder.EstimatedShipDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.EstimatedShipDate);
        $('#estimatedshipdate').datepicker('setDate', $scope.tmpOrder.EstimatedShipDateString);
        $scope.tmpOrder.FinalPaymentDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.FinalPaymentDate);
        $('#finalpaymentdate').datepicker('setDate', $scope.tmpOrder.FinalPaymentDateString);

        if ($scope.tmpOrder.Salesman2 == null) $scope.tmpOrder.Salesman2 = $scope.Salesman2_NA.ID;
        if ($scope.tmpOrder.RSM == null) $scope.tmpOrder.RSM = $scope.Salesman2_NA.ID;

        $("#orderContent").modal({ 'backdrop': 'static' });
        $("#orderContent").draggable({ handle: ".modal-header", cursor: "move" });
        $timeout(function () { $scope.$apply(); });
    }

    $scope.editOrderProducts = function () {
        if ($scope.tmpOrder.Salesman == null) {
            showError("Please select a Dealer / DSE.")
            return;
        }

        if ($scope.tmpOrder.OrderDateString == '') {
            showError("Please enter Order Date.")
            return;
        }

        if ($scope.tmpOrder.Salesman == $scope.tmpOrder.Salesman2) {
            showError("Dealer / DSE cannot split order with himself.")
            return;
        }

        $scope.productGrid = ($scope.orderHasTooling() ? $scope.productGridWithTooling : $scope.productGridWithoutTooling);
        $scope.productGrid.updateData();
        $scope.productTotalGrid.updateData($scope.productGrid.getSelectedProducts());
        $scope.productSellPriceGrid.updateData($scope.tmpOrder ? $scope.tmpOrder.SalePrice : 0);

        $("#editOrder").modal({ 'backdrop': 'static' });
        $("#editOrder").draggable({ handle: ".modal-header", cursor: "move" });
    };

    $scope.calculateOrder = function (order, callAfterRecalculation) {
        Connection.calculateOrder($scope.user, order,
            function (response) {
                if (response.Status == true) {
                    if (callAfterRecalculation)
                        callAfterRecalculation(normalizeOrder(response.Data.Order));
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.recalculateOrder = function (order) {
        Connection.recalculateOrder($scope.user, order.ID,
            function (response) {
                if (response.Status == true) {
                    $scope.filteredSalesmen = $scope.salesmen = response.Data.Salesmen;
                    $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                    $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);

                    $scope.order = $scope.tmpOrder = normalizeOrder(response.Data.Order);
                    var index = _.findIndex($scope.orders, function (order) { return order.ID == $scope.order.ID; });
                    $scope.orders[index] = $scope.order;
                    
                    notifySuccess('Order ' + order.PONumber + ' is recalculated.');

                    layoutOrderGrid();
                    $scope.orderGrid.updateData($scope.orders, $scope.filter);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        ); 
    }

    $scope.onCalculationChanged = function () {
        $scope.calculateOrder($scope.tmpOrder, updateOrderData);
    }

    $scope.updateOrderInputData = function () {
        $scope.productSellPriceGrid.stopEdit();
        var totalListPrice = $scope.productTotalGrid.getData()[0].Value;
        if (!totalListPrice) {
            showError("Please select product(s) from the 'Machines and Options' list.")
            return;
        }

        var salePrice = parseFloat($scope.productSellPriceGrid.getData()[0].Value);
        if (!salePrice || isNaN(salePrice)) {
            showError("Please enter valid Selling Price according to Order Confirmation.") 
            return;
        }
        //if (salePrice > totalListPrice) {
        //    showError("Selling Price cannot exceed Total List Price.")
        //    return;
        //}
        $scope.tmpOrder.OrderItems = $scope.productGrid.getSelectedProducts();
        $scope.tmpOrder.TotalListPrice = totalListPrice;
        $scope.tmpOrder.SalePrice = salePrice;
        $scope.tmpOrder.OrderDate = $scope.tmpOrder.OrderDateString;
        $scope.tmpOrder.EstimatedShipDate = $scope.tmpOrder.EstimatedShipDateString;
        $scope.tmpOrder.FinalPaymentDate = $scope.tmpOrder.FinalPaymentDateString;

        $scope.calculateOrder($scope.tmpOrder, updateOrderData);
        $("#editOrder").modal('hide');
    }

    $scope.payRSMCommissionToggled = function () {
        $scope.orderOutputDataGrid.updateData($scope.tmpOrder);
    }

    $scope.approveOrder = function () {
        $scope.tmpOrder.Status = $scope.statuses.approved;
        $scope.tmpOrder.ApprovedBy = $scope.user.Name;
        $scope.tmpOrder.ApprovedDate = new Date();
        notifySuccess("Order " + $scope.tmpOrder.PONumber + " is approved.", 2000);
        $scope.saveOrder();
    }

    $scope.undoApproveOrder = function () {
        $scope.tmpOrder.Status = $scope.statuses.pending;
        $scope.tmpOrder.ApprovedBy = "";
        $scope.tmpOrder.ApprovedDate = null;
        notifySuccess("Order " + $scope.tmpOrder.PONumber + " status reverted to 'pending'.", 2000);
        $scope.saveOrder();
    }

    $scope.releaseOrder = function () {
        $scope.tmpOrder.Status = $scope.statuses.released;
        $scope.tmpOrder.ReleasedBy = $scope.user.Name;
        $scope.tmpOrder.ReleasedDate = new Date();
        notifyWarning("Order " + $scope.tmpOrder.PONumber + " is released.", 2000);
        $scope.saveOrder();
    }

    $scope.undoReleaseOrder = function () {
        $scope.tmpOrder.Status = $scope.statuses.pending;
        $scope.tmpOrder.ApprovedBy = "";
        $scope.tmpOrder.ApprovedDate = null;
        $scope.tmpOrder.ReleasedBy = "";
        $scope.tmpOrder.ReleasedDate = null;
        notifySuccess("Order " + $scope.tmpOrder.PONumber + " status reverted to 'pending'.", 2000);
        $scope.saveOrder();
    }

    $scope.markPaidOrder = function () {
        $("#editPayrollDate").modal({ 'backdrop': 'static' });
        $("#editPayrollDate").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.undoMarkPaidOrder = function () {
        $scope.tmpOrder.Status = $scope.statuses.released;
        $scope.tmpOrder.PaidBy = "";
        $scope.tmpOrder.PayDate = null;
        $scope.tmpOrder.PayrollDate = null;
        notifySuccess("Order " + $scope.tmpOrder.PONumber + " status reverted to 'released'.", 2000);
        $scope.saveOrder();
    }

    $scope.saveOrderOrComment = function () {
        if ($scope.canSaveOrder())
            $scope.saveOrder();
        else
            $scope.saveOrderComment();
    }

    $scope.saveOrder = function () {
        $scope.orderOutputDataGrid.stopEdit();

        if ($scope.tmpOrder.Salesman == null) {
            showError("Please select a Dealer / DSE.")
            return;
        }
        if ($scope.tmpOrder.Salesman == $scope.tmpOrder.Salesman2) {
            showError("Dealer / DSE cannot split order with himself.")
            return;
        }
        if ($scope.tmpOrder.CustomerID == -1) {
            showError("Please select a Customer.")
            return;
        }
        if (!$scope.tmpOrder.PONumber) {
            showError("Please enter Job Number.")
            return;
        }
        else {
            // check if order with the same PO Number exists
            if (_.findIndex($scope.orders, function (order) { return order.PONumber == $scope.tmpOrder.PONumber && order.ID != $scope.tmpOrder.ID; }) != -1) {
                showError("Order with the Job Number " + $scope.tmpOrder.PONumber + " already exists. Please enter a different value.")
                return;
            }
        }
        if (!$scope.tmpOrder.TrackingNumber) {
            showError("Please enter SAP Number.")
            return;
        }
        if ($scope.tmpOrder.TotalListPrice == 0) {
            showError("Please select product(s).")
            return;
        }

        $scope.tmpOrder.OrderDate = $scope.tmpOrder.OrderDateString;// =$scope.order.OrderDate instanceof Date ? $.datepicker.formatDate('mm/dd/y', $scope.order.OrderDate) : $scope.order.OrderDate;
        $scope.tmpOrder.EstimatedShipDate = $scope.tmpOrder.EstimatedShipDateString;
        $scope.tmpOrder.FinalPaymentDate = $scope.tmpOrder.FinalPaymentDateString;

        $scope.tmpOrder.ApprovedDate = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.ApprovedDate);
        $scope.tmpOrder.ReleasedDate = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.ReleasedDate);
        $scope.tmpOrder.PayDate = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.PayDate);
        //$scope.tmpOrder.PayrollDate = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.PayrollDate);

        if ($scope.tmpOrder.Salesman2 == $scope.Salesman2_NA.ID) $scope.tmpOrder.Salesman2 = null;
        if ($scope.tmpOrder.RSM == $scope.Salesman2_NA.ID) $scope.tmpOrder.RSM = null;

        $("#orderContent").modal('hide');

        Connection.saveOrder($scope.user, $scope.tmpOrder,
            function (response) {
                if (response.Status == true) {
                   //$("#orderContent").modal('hide');
                    $scope.order = normalizeOrder(response.Data.Order);
                    $scope.filteredSalesmen = $scope.salesmen = response.Data.Salesmen;
                    $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                    $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                    var index = _.findIndex($scope.orders, function (order) { return order.ID == $scope.order.ID; });
                    if(index >= 0)
                        $scope.orders[index] = $scope.order;
                    else {
                        $scope.orders.push($scope.order);
                    }

                    $scope.orderGrid.updateData($scope.orders, $scope.filter);
                    $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * 0.99*/);
                    layoutOrderGrid();
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.saveOrderComment = function () {
        Connection.saveOrder($scope.user, $scope.tmpOrder,
            function (response) {
                if (response.Status == true) {
                    $("#orderContent").modal('hide');
                    $scope.order = normalizeOrder(response.Data.Order);
                    var index = _.findIndex($scope.orders, function (order) { return order.ID == $scope.order.ID; });
                    $scope.orders[index] = $scope.order;
                    $scope.orderGrid.updateData($scope.orders, $scope.filter);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.deleteOrder = function (order) {
        Connection.deleteOrder($scope.user, order,
            function (response) {
                if (response.Status == true) {
                    $scope.filteredSalesmen = $scope.salesmen = response.Data.Salesmen;
                    $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                    $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                    var index = _.findIndex($scope.orders, function (o) { return o.ID == order.ID; });
                    $scope.orders.splice(index, 1);
                    $scope.order = index < $scope.orders.length ? $scope.orders[index] : null;
                    $scope.orderGrid.updateData($scope.orders, $scope.filter);
                    $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * 0.99*/);
                    layoutOrderGrid();
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    updateOrderData = function (order, notify) {
        $scope.tmpOrder = order;
        $scope.tmpOrder.OrderDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.OrderDate);
        $scope.tmpOrder.EstimatedShipDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.EstimatedShipDate);
        $scope.tmpOrder.FinalPaymentDateString = $.datepicker.formatDate('mm/dd/y', $scope.tmpOrder.FinalPaymentDate);
        $scope.orderInputDataGrid.updateData(order);
        $scope.orderOutputDataGrid.updateData(order);
        $timeout(function () { $scope.$apply(); });
    }

    onProductSelected = function (orderItems) {
        $scope.productTotalGrid.updateData(orderItems);
    }

    findOrderFor = function (orderID) {
        return _.find($scope.orders, function (o) { return o.ID == orderID; });
    }

    isNewOrder = function (order) {
        return _.find($scope.orders, function (o) { return o.ID == order.ID; }) == null;
    }

    $scope.isAdministrator = function () {
        return $scope.user.Role == $scope.roles.administrator;
    }

    $scope.canEditOrder = function () {
        if (!$scope.tmpOrder) return false;
        return ($scope.permissions.CanEditOrder || $scope.permissions.CanApproveOrder) && $scope.tmpOrder.Status == $scope.statuses.pending;
    }

    $scope.canApproveOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.permissions.CanApproveOrder && $scope.tmpOrder.Status == $scope.statuses.pending && !isNewOrder($scope.tmpOrder);
    }

    $scope.canEditCommissions = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.permissions.CanEditOrder && $scope.tmpOrder.Status == $scope.statuses.pending;
    }

    $scope.canUndoApproveOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.tmpOrder.Status == $scope.statuses.approved && (($scope.permissions.CanApproveOrder && $scope.tmpOrder.ApprovedBy == $scope.user.Name) || $scope.permissions.CanEditOrder);
    }

    $scope.canReleaseOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.permissions.CanReleaseOrder && $scope.tmpOrder.Status == $scope.statuses.approved && $scope.tmpOrder.ApprovedBy != $scope.user.Name;
    }

    $scope.canUndoReleaseOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.tmpOrder.Status == $scope.statuses.released && (($scope.permissions.CanReleaseOrder &&  $scope.tmpOrder.ReleasedBy == $scope.user.Name) || $scope.permissions.CanEditOrder);
    }

    $scope.canMarkPaidOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.permissions.CanPayOrder && $scope.tmpOrder.Status == $scope.statuses.released;
    }

    $scope.canUndoMarkPaidOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.permissions.CanPayOrder && $scope.tmpOrder.Status == $scope.statuses.paid;
    }

    $scope.canSaveOrder = function () {
        if (!$scope.tmpOrder) return false;
        return $scope.canEditOrder();
    }

    $scope.showSalesmenList = function () {
        $scope.salesmenListGrid.updateData($scope.salesmen);
        $("#salesmenList").modal({ 'backdrop': 'static' });
        $("#salesmenList").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.showSalesList = function () {
        $scope.salesListGrid.updateData($scope.salesmen, $scope.selectedYear);
        $("#salesList").modal({ 'backdrop': 'static' });
        $("#salesList").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.showCustomerList = function () {
        $scope.customerListGrid.updateData($scope.customers);
        $("#customerList").modal({ 'backdrop': 'static' });
        //$("#customerList").position({ my: "center bottom", at: "center bottom", of: window });
        $("#customerList").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.showProductList = function () {
        $scope.productListGrid.updateData($scope.products);
        $("#productList").modal({ 'backdrop': 'static' });
        $("#productList").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.showSettings = function () {
        $scope.tmpSettings = $.extend(true, {}, $scope.settings);
        $("#settingsDialog").modal({ 'backdrop': 'static' });
        $("#settingsDialog").draggable({ handle: ".modal-header", cursor: "move" });
    }

    $scope.saveSettings = function () {
        if (isNaN($scope.tmpSettings.IdleTimeout) || $scope.tmpSettings.IdleTimeout < 1 || $scope.tmpSettings.IdleTimeout > 60) {
            showError('Please enter valid timeout interval.');
            return;
        }
        $("#settingsDialog").modal('hide');
        saveCookie($cookies, "bystronic_client_settings", JSON.stringify($scope.settings = $scope.tmpSettings));
    }

    $scope.logOut = function () {
        window.location.replace("login.html");
    }
    $scope.clearOrderFilters = function () {
        $scope.filter = { Status: null, Region: null, Product: null, StartDate: null, EndDate: null, ShipStartDate: null, ShipEndDate: null };
        $timeout(function () { $scope.$apply(); });
    }

    $scope.clearOrderFilter = function (field) {
        if (field == 'status') {
            $scope.filter.Status = null;
        }
        else if (field == 'region') {
            $scope.filter.Region = null;
            $scope.filteredSalesmen = $scope.salesmen;
        }
        else if (field == 'salesman')
            $scope.filter.Salesman = null;
        else if (field == 'product')
            $scope.filter.Product = null;
        else if (field == 'daterange') {
            $scope.filter.StartDate = null;
            $scope.filter.EndDate = null;
            $scope.filter.ShipStartDate = null;
            $scope.filter.ShipEndDate = null;
        }
        else
            $scope.clearOrderFilters();
        $scope.filterChanged();
    }

    $scope.regionFilterChanged = function () {
        $scope.filteredSalesmen = _.filter($scope.salesmen, function (s) {
            if ($scope.filter.Region) {
                var isFound = false;
                $.each($scope.filter.Region, function (index, region) {
                    if (s.Region == region) {
                        isFound = true;
                        return;
                    }
                });
                return isFound;
            }
            else 
                return true;
        });
        $scope.filterChanged();
    }

    $scope.filterChanged = function () {
        $scope.orderGrid.updateData($scope.orders, $scope.filter);
        $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * 0.99*/);
        $("#order_grid").parents('div.ui-jqgrid-bdiv').css("max-height", '45vh');
    }

    $scope.orderHasTooling = function () {
        return $scope.tmpOrder && $scope.tmpOrder.TemplateID == 3; // Bending (Pressbrake)
    }

    $scope.salesmanSelectionChanged = function () {
        if ($scope.tmpOrder.Salesman == $scope.tmpOrder.Salesman2) {
            showError("Dealer / DSE cannot split order with himself.")
            return;
        }
        if ($scope.tmpOrder.TotalListPrice > 0)
            $scope.calculateOrder($scope.tmpOrder, updateOrderData);
    };

    $scope.customerSelectionChanged = function () {
        if (!$scope.tmpOrder) return;
        if ($scope.tmpOrder.CustomerID == 999) { // Add customer
            $scope.editCustomerDialog({ Name: '', Address: '', City: '', State: '', ZipCode: '', SapNumber: '', Order: $scope.tmpOrder, isNew: true });
        }
        else {
            $scope.tmpOrder.TrackingNumber = _.find($scope.customers, function (c) { return c.ID == $scope.tmpOrder.CustomerID; }).SapNumber;
        }
    };


 /////////////////////////////    G R I D  C O L U M N S   /////////////////////////////////////////////////////////////////////////

    $scope.changeOrderView = function (view) {
        if (view.Name !== $scope.currentView) {
            sessionStorage.currentView = $scope.currentView = view.Name;
            $scope.columns = view.Columns.split(',');
            onOrderGridStructureChanged();
        }
    };

    $scope.deleteOrderView = function (view) {
        showYesNoMessage('View', 'Do you want to delete view ' + $scope.currentView + '?',
            function () {
                Connection.deleteView($scope.user, $scope.currentView,
                    function (response) {
                        if (response.Status == true) {
                            deleteViewByName($scope.currentView);
                            $scope.changeOrderView(getDefaultView());
                        }
                        else {
                            showError(response.Data.Error);
                        }
                    },
                    function (errorMessage) {
                        showError(errorMessage);
                    }
                );
            }
        );
        if (view.Name !== $scope.currentView) {
            $scope.currentView = view.Name;
            $scope.columns = view.Columns.split(',');
            onOrderGridStructureChanged();
        }
    };

    onOrderGridStructureChanged = function () {
        // reload order table
        $("#order_grid").GridUnload();
        $scope.orderGrid = new OrderGrid($scope, '#order_grid', onOrderDoubleClicked);
        jQuery("#order_grid").jqGrid('bindKeys');

        $('#order_grid').setGridWidth($('#order_grid_wrapper').width()/* * 0.99*/);
        layoutOrderGrid();
        $scope.orderGrid.updateData($scope.orders, $scope.filter);
    };

    getViewByName = (name) => _.find($scope.views, function (v) { return v.Name == name; });

    getDefaultView = () => getViewByName('Default');

    getCurrentView = () => getViewByName($scope.currentView);

    viewExists = (name) => getViewByName(name) != null;

    deleteViewByName = (name) => {
        let view = getViewByName(name);
        if (view) $scope.views = _.without($scope.views, view);
    };

    $scope.getAllAvailableColumns = function () {
        var columns = $scope.IsCanadianVersion
            ? ['PONumber', 'SalesOrder', 'Template', 'Customer', 'OrderDate', 'EstimatedShipDate', 'FinalPaymentDate', 'SalesmanName', 'CostCenter', 'TotalListPrice', 'SalePrice', 'DiscountAmount', 'DiscountPercent']
            : ['PONumber', 'SalesOrder', 'Template', 'Customer', 'OrderDate', 'EstimatedShipDate', 'FinalPaymentDate', 'SalesmanName', 'CostCenter', 'RegionalManagerName', 'RSMCostCenter', 'TotalListPrice', 'SalePrice', 'DiscountAmount', 'DiscountPercent'];

        if ($scope.user.Role != $scope.roles.pm)
            columns.push('DealerCommissionInfo');
        if (!($scope.IsCanadianVersion || $scope.user.Role == $scope.roles.dealer || $scope.user.Role == $scope.roles.dse || $scope.user.Role == $scope.roles.pm))
            columns.push('RegionalManagerCommission');
        if (!($scope.user.Role == $scope.roles.dealer || $scope.user.Role == $scope.roles.dse || $scope.user.Role == $scope.roles.rsm))
            columns.push('ProductManagerCommission');
        columns.push('Status');
        columns.push('ApprovedInfo');
        columns.push('ReleasedInfo');
        columns.push('PaidInfo');
        columns.push('Comment');
        columns.push('YtdSale');
        columns.push('SaleGoal');

        return columns;
    }

    $scope.availableColumns = [
        { ID: 'PONumber', Name: 'Job' },
        { ID: 'SalesOrder', Name: 'Sales Order' },
        { ID: 'Template', Name: 'Product' },
        { ID: 'Customer', Name: 'Customer' },
        { ID: 'OrderDate', Name: 'Date' },
        { ID: 'EstimatedShipDate', Name: 'Est. Ship Date' },
        { ID: 'FinalPaymentDate', Name: 'Date of Final Pmt.' },
        { ID: 'TotalListPrice', Name: 'List Price' },
        { ID: 'SalePrice', Name: 'Selling Price' },
        { ID: 'DiscountAmount', Name: 'Discount' },
        { ID: 'DiscountPercent', Name: 'Disc. %' },
        { ID: 'SalesmanName', Name: 'Dealer / DSE' },
        { ID: 'RegionalManagerName', Name: 'RSM' },
        { ID: 'CostCenter', Name: 'DSE Cost Center' },
        { ID: 'RSMCostCenter', Name: 'RSM Cost Center' },
        { ID: 'DealerCommissionInfo', Name: 'Dealer/DSE Comm.' },
        { ID: 'RegionalManagerCommission', Name: 'RSM Comm.' },
        { ID: 'ProductManagerCommission', Name: 'PM Comm.' },
        { ID: 'Status', Name: 'Status' },
        { ID: 'ApprovedInfo', Name: 'Approved' },
        { ID: 'ReleasedInfo', Name: 'Released' },
        { ID: 'PaidInfo', Name: 'Paid' },
        { ID: 'Comment', Name: 'Comment' },
        { ID: 'YtdSale', Name: 'YTD Sale' },
        { ID: 'SaleGoal', Name: 'Sale Goal' }
    ];

    $scope.getColumnHeaderFor = function (columnID) {
        return _.find($scope.availableColumns, function (c) { return c.ID == columnID; }).Name;
    };

    $scope.getColumnNameFor = function (columnHeader) {
        return _.find($scope.availableColumns, function (c) { return c.Name == columnHeader; }).ID;
    };

    $scope.editColumnsDialog = function () {
        $scope.columnList.updateData($scope.columns);
        $scope.newView = $scope.currentView;
        $("#columnList").modal({ 'backdrop': 'static' });
        $("#columnList").draggable({ handle: ".modal-header", cursor: "move" });
        $timeout(function () { $scope.$apply(); });
    }

    $scope.saveColumns = function () {
        $scope.columns = $scope.columnList.getSelectedColumns();
        var columnsAsString = $scope.columns.join(',');
        Connection.saveColumns($scope.user, $scope.currentView, $scope.newView, columnsAsString,
            function (response) {
                if (response.Status == true) {
                    if (viewExists($scope.newView)) {
                        getViewByName($scope.newView).Columns = columnsAsString;
                    }
                    else {
                        $scope.views.push({ Name: $scope.newView, Columns: columnsAsString });
                    }
                    sessionStorage.currentView = $scope.currentView = $scope.newView;
                    $("#columnList").modal('hide');
                    onOrderGridStructureChanged();
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    };

    $scope.saveColumnsAfterDragAndDrop = function () {
        var columnsAsString = $scope.columns.join(',');
        Connection.saveColumns($scope.user, $scope.currentView, $scope.currentView, columnsAsString,
            function (response) {
                if (response.Status == true) {
                    getCurrentView().Columns = columnsAsString;
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

 /////////////////////////////    S A L E S M E N   ////////////////////////////////////////////////////////////////////////////////

    $scope.editSalesmanDialog = function (salesman) {
        $scope.newSalesman = salesman ? salesman : { ID: '', Name: '', Role: '', Region: '', isNew: true };
        $scope.salesmanID = $scope.newSalesman.ID;

        $("#addSalesman").modal({ 'backdrop': 'static' });
        $("#addSalesman").draggable({ handle: ".modal-header", cursor: "move" });
        $timeout(function () {
            $scope.salesGoalSelectionChanged($scope.newSalesman.ID);
            $scope.$apply();
        });
    }

    $scope.addSalesman = function () {
        $scope.updateSalesman($scope.newSalesman);
    }

    $scope.updateSalesman = function (salesman) {
        if (!salesman.ID) {
            showError("Please enter Dealer / DSE Login.")
            return;
        }
        if (!salesman.Name) {
            showError("Please enter Dealer / DSE Name.")
            return;
        }
        if (!salesman.Role) {
            showError("Please select Dealer / DSE Role.")
            return;
        }
        if (!salesman.Region) {
            showError("Please select Dealer / DSE Region.")
            return;
        }

        if (salesman.isNew) {
            if (_.find($scope.salesmen, function (s) { return s.ID == $scope.newSalesman.ID; }) != null) {
                showError("Deale / DSE with login " + $scope.newSalesman.ID + " already exists.")
                return;
            }
            if (_.find($scope.salesmen, function (s) { return s.Name == $scope.newSalesman.Name; }) != null) {
                showError("Deale / DSE with name " + $scope.newSalesman.Name + " already exists.")
                return;
            }
            Connection.addSalesman($scope.user, salesman,
                function (response) {
                    if (response.Status == true) {
                        $scope.salesmen = response.Data.Salesmen;
                        $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                        $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                        $scope.filteredSalesmen = _.filter($scope.salesmen, function (s) { return s.Region == $scope.filter.Region; });
                        $scope.salesmenListGrid.updateData($scope.salesmen);
                        $("#addSalesman").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
        else {
            Connection.updateSalesman($scope.user, $scope.salesmanID, salesman,
                function (response) {
                    if (response.Status == true) {
                        $scope.salesmen = response.Data.Salesmen;
                        $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                        $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                        $scope.filteredSalesmen = _.filter($scope.salesmen, function (s) { return s.Region == $scope.filter.Region; });
                        $scope.salesmenListGrid.updateData($scope.salesmen);
                        $("#addSalesman").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
    }

    $scope.deleteSalesman = function (salesman) {
        if (_.find($scope.orders, function (o) { return o.Salesman == salesman.ID || o.Salesman2 == salesman.ID; }) != null) {
            showError("Cannot delete " + salesman.Name + ", because orders for this Dealer / DSE exist.");
            return;
        }
        Connection.deleteSalesman($scope.user, salesman,
            function (response) {
                if (response.Status == true) {
                    $scope.salesmen = response.Data.Salesmen;
                    $scope.Salesman2_NA = { ID: "NA", Name: " N/A", Role: "RSM" };
                    $scope.salesmen2 = $.merge($.merge([], [$scope.Salesman2_NA]), $scope.salesmen);
                    $scope.filteredSalesmen = _.filter($scope.salesmen, function (s) { return s.Region == $scope.filter.Region; });
                    $scope.salesmenListGrid.updateData($scope.salesmen);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.salesGoalSelectionChanged = function(salesmanID) {
        if (!salesmanID) {
            $scope.salesGoalTarget = null;
            return;
        }
        var salesman = _.find($scope.salesmen, function (s) { return s.ID == salesmanID; });
        if (!salesman.SalesGoals) {
            $scope.salesGoalTarget = null;
            return;
        }
        var salesGoal = _.find(salesman.SalesGoals, function (sg) { return sg.Year == $scope.year; });
        $scope.salesGoalTarget = salesGoal ? salesGoal.Target : null;
    }

    $scope.updateSalesGoal = function (salesmanID) {
        var salesman = _.find($scope.salesmen, function (s) { return s.ID == salesmanID; });
        if (!salesman.SalesGoals)
            salesman.SalesGoals = [];
        var salesGoal = _.find(salesman.SalesGoals, function (sg) { return sg.Year == $scope.year; });
        if (salesGoal)
            salesGoal.Target = parseFloat($scope.salesGoalTarget);
        else
            salesman.SalesGoals.push({ Year: $scope.year, Target: $scope.salesGoalTarget });
        $scope.newSalesman = salesman;
    };

    $scope.salesYearSelectionChanged = function () {
        $scope.salesListGrid.updateData($scope.salesmen, $scope.selectedYear);
    };


 /////////////////////////////    C U S T O M E R S   ////////////////////////////////////////////////////////////////////////////////

    $scope.editCustomerDialog = function (customer) {
        $scope.newCustomer = customer ? customer : { Name: '', Address: '', City: '', State: '', ZipCode: '', SapNumber: '', isNew: true };
        $("#addCustomer").modal({ 'backdrop': 'static' });
        $("#addCustomer").draggable({ handle: ".modal-header", cursor: "move" });
        $timeout(function () { $scope.$apply(); });
    }

    $scope.addCustomer = function () {
        $scope.updateCustomer($scope.newCustomer);
    }

    $scope.updateCustomer = function (customer) {
        if (!customer.Name) {
            showError("Please enter Customer Name.")
            return;
        }
        if (!customer.SapNumber) {
            showError("Please enter Customer SAP Number.")
            return;
        }
        if (!customer.Address) {
            showError("Please enter Customer Address.")
            return;
        }
        if (!customer.City) {
            showError("Please enter Customer City.")
            return;
        }
        if (!customer.State) {
            showError("Please enter Customer State/Province.")
            return;
        }
        if (!customer.ZipCode) {
            showError("Please enter Customer ZIP Code.")
            return;
        }

        if (customer.isNew) {
            if (_.find($scope.customers, function (c) { return c.Name == $scope.newCustomer.Name; }) != null) {
                showError("Customer " + $scope.newCustomer.Name + " already exists.")
                return;
            }
            if (_.find($scope.customers, function (c) { return c.SapNumber == $scope.newCustomer.SapNumber; }) != null) {
                showError("Customer with SAP Number " + $scope.newCustomer.SapNumber + " already exists.")
                return;
            }
            Connection.addCustomer($scope.user, customer,
                function (response) {
                    if (response.Status == true) {
                        $scope.customers = response.Data.Customers;
                        $scope.tmpCustomers = _.sortBy($.extend(true, [], $scope.customers), 'Name');
                        $scope.tmpCustomers.push({ ID: 999, Name: 'Add...' });
                        if (customer.Order) {
                            var selectedCustomer = _.find($scope.customers, function (c) { return c.Name == customer.Name; });
                            customer.Order.CustomerID = selectedCustomer.ID;
                            customer.Order.TrackingNumber = selectedCustomer.SapNumber;
                        }
                        $scope.customerListGrid.updateData($scope.customers);
                        $("#addCustomer").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
        else {
            Connection.updateCustomer($scope.user, customer,
                function (response) {
                    if (response.Status == true) {
                        $scope.customers = response.Data.Customers;
                        $scope.tmpCustomers = _.sortBy($.extend(true, [], $scope.customers), 'Name');
                        $scope.tmpCustomers.push({ ID: 999, Name: 'Add...' });
                        $scope.customerListGrid.updateData($scope.customers);
                        $("#addCustomer").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
    }

    $scope.deleteCustomer = function (customer) {
        if (_.find($scope.orders, function (o) { return o.CustomerID == customer.ID; }) != null) {
            showError("Cannot delete " + customer.Name + ", because orders for this customer exist.");
            return;
        }
        Connection.deleteCustomer($scope.user, customer,
            function (response) {
                if (response.Status == true) {
                    $scope.customers = response.Data.Customers;
                    $scope.tmpCustomers = _.sortBy($.extend(true, [], $scope.customers), 'Name');
                    $scope.tmpCustomers.push({ ID: 999, Name: 'Add...' });
                    $scope.customerListGrid.updateData($scope.customers);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

 /////////////////////////////    P R O D U C T S   ////////////////////////////////////////////////////////////////////////////////

    $scope.addProductDialog = function () {
        $scope.editProductDialog({ Name: '', Type: 0, Value: 0.00, ProductTypeID: $scope.tmpOrder.TemplateID, isNew: true, addInline: true });
    }
    
    $scope.editProductDialog = function (product) {
        $scope.newProduct = product ? product : { Name: '', Type: 0, Value: 0.00, ProductTypeID: -1, isNew: true };
        $("#addProduct").modal({ 'backdrop': 'static' });
        $("#addProduct").draggable({ handle: ".modal-header", cursor: "move" });
        $timeout(function () { $scope.$apply(); });
    }

    $scope.addProduct = function () {
        $scope.updateProduct($scope.newProduct);
    }

    $scope.updateProduct = function (product) {
        if (!product.Name) {
            showError("Please enter Product Name.")
            return;
        }
        if (product.ProductTypeID == -1) {
            showError("Please select Product Type.")
            return;
        }

        if (!product.Value) {
            product.Value = 0.00;
        }

        if (product.isNew) {
            if (_.find($scope.products, function (p) { return p.Name == $scope.newProduct.Name; }) != null) {
                showError("product " + $scope.newProduct.Name + " already exists.")
                return;
            }
            Connection.addProduct($scope.user, product,
                function (response) {
                    if (response.Status == true) {
                        $scope.products = response.Data.Products;
                        if (product.addInline) {
                            $scope.productGrid = ($scope.orderHasTooling() ? $scope.productGridWithTooling : $scope.productGridWithoutTooling);
                            $scope.productGrid.updateData(product);
                            $scope.productTotalGrid.updateData();
                        }
                        else
                            $scope.productListGrid.updateData($scope.products);

                        $("#addProduct").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
        else {
            Connection.updateProduct($scope.user, product,
                function (response) {
                    if (response.Status == true) {
                        $scope.products = response.Data.Products;
                        $scope.productListGrid.updateData($scope.products);
                        $("#addProduct").modal('hide');
                    }
                    else {
                        showError(response.Data.Error);
                    }
                },
                function (errorMessage) {
                    showError(errorMessage);
                }
            );
        }
    }

    $scope.deleteProduct = function (product) {
        if (_.find($scope.orders, function (o) { return orderContainsProduct(o, product); }) != null) {
            showError("Cannot delete " + product.Name + ", because orders with this product exist.");
            return;
        }
        Connection.deleteProduct($scope.user, product,
            function (response) {
                if (response.Status == true) {
                    $scope.products = response.Data.Products;
                    $scope.productListGrid.updateData($scope.products);
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    orderContainsProduct = function (order, product) {
        return _.find(order.OrderItems, function (o) { return o.ProductID == product.ID; }) != null;
    }

/////////////////////////////    F O R M U L A S   ////////////////////////////////////////////////////////////////////////////////

    $('#testformula').on('shown.bs.collapse', function (e) {
        $scope.editor.resize();
        $scope.editor.renderer.updateFull();
    });
    $('#testformula').on('hidden.bs.collapse', function (e) {
        $scope.editor.resize();
        $scope.editor.renderer.updateFull();

        $scope.test.Output = { DiscountAmount: '', DiscountPercent: '', DealerCommission: '', RSMCommission: '', PMCommission: '', Error: '' };
        $timeout(function () { $scope.$apply(); });
    });

    $scope.showFormulaEditor = function (templateName) {
        $("#optionsmenu").dropdown("toggle");
        $('#testformula').collapse("hide");
        $scope.templateID = _.find($scope.templates, function (t) { return t.Name == templateName; }).ID;
        $scope.test = {
            TemplateID: $scope.templateID, 
            Input: { TypeOfSale: 0, OrderDate: $.datepicker.formatDate('mm/dd/yy', new Date()),  ListPrice: '0', SalePrice: 0, YtdSale: 0, EquipmentPrice: 0, ToolingPrice: 0, IsXpert40orXactMachine: false },
            Output: { DiscountAmount: '', DiscountPercent: '', DealerCommission: '', RSMCommission: '', PMCommission: '', Error: '' }
        };
        
        Connection.getFormula($scope.user, $scope.templateID,
            function (response) {
                if (response.Status == true) {
                    $scope.formula = response.Data.Formula;
                    $scope.templateName = templateName;
                    $scope.editor = ace.edit("editor");
                    $scope.editor.session.setMode("ace/mode/java");
                    //editor.setTheme("ace/theme/cobalt");
                    $scope.editor.setValue($scope.formula, -1);

                    $scope.formulaChanged = false;
                    $scope.editor.getSession().on('change', function () {
                        $scope.formulaChanged = true;
                        $timeout(function () { $scope.$apply(); });
                    });
                    
                    $("#formulaEditor").modal({ 'backdrop': 'static' });
                    $("#formulaEditor").draggable({ handle: ".modal-header", cursor: "move" });
                    $timeout(function () { $scope.$apply(); });
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.saveFormula = function () {
        $scope.formula = $scope.editor.getValue();
        Connection.saveFormula($scope.user, $scope.templateID, $scope.formula,
            function (response) {
                if (response.Status == true) {
                    $scope.formulaChanged = false;
                    $timeout(function () { $scope.$apply(); });
                    notifySuccess("Commission calculation formula for " + $scope.templateName + " orders is updated.");
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.restoreDefaultFormula = function () {
        Connection.restoreDefaultFormula($scope.user, $scope.templateID,
            function (response) {
                if (response.Status == true) {
                    $scope.formula = response.Data.Formula;
                    $scope.editor.setValue($scope.formula, -1);

                    $scope.formulaChanged = false;
                    $scope.test.Output.Error = '';
                    $timeout(function () { $scope.$apply(); });
                    notifySuccess("Default calculation formula for " + $scope.templateName + " orders is restored.");
                }
                else {
                    showError(response.Data.Error);
                }
            },
            function (errorMessage) {
                showError(errorMessage);
            }
        );
    }

    $scope.testFormula = function () {
        var formula = $scope.editor.getValue();

        if ($scope.test.TemplateID == 3) {
            if (parseFloat($scope.test.Input.EquipmentPrice) <= 0) {
                showFormulaTestError("Total Equipment Price should be greater than 0.")
                return;
            }
            if (parseFloat($scope.test.Input.ToolingPrice) < 0) {
                showFormulaTestError("Total Tooling Price should be greater or equal to 0.")
                return;
            }
            $scope.test.Input.ListPrice = parseFloat($scope.test.Input.EquipmentPrice) + parseFloat($scope.test.Input.ToolingPrice);
        }
        else { 
            if (!$scope.test.Input.ListPrice || parseFloat($scope.test.Input.ListPrice) <= 0) {
                showFormulaTestError("Total List Price should be greater than 0.")
                return;
            }
        }

        if (!$scope.test.Input.SalePrice || parseFloat($scope.test.Input.SalePrice <= 0)) {
            showFormulaTestError("Selling Price should be greater than 0.")
            return;
        }
        //if (parseFloat($scope.test.Input.SalePrice) > parseFloat($scope.test.Input.ListPrice)) {
        //    showFormulaTestError("Selling Price cannot exceed Total List Price.")
        //    return;
        //}

        Connection.testFormula($scope.user, formula, $scope.test.Input,
            function (response) {
                if (response.Status == true) {
                    var output = response.Data.Output;
                    if (output.Error)
                        $scope.test.Output.Error = output.Error; 
                    else
                        $scope.test.Output = output;
                    $timeout(function () { $scope.$apply(); });
                }
                else {
                    $scope.test.Output.Error = response.Data.Error;
                    $timeout(function () { $scope.$apply(); });
                }
            },
            function (errorMessage) {
                showFormulaTestError(errorMessage);
            }
        );
    }

    $scope.closeFormulaEditor = function () {
        if ($scope.formulaChanged) {
            showYesNoMessage('Commission Calculation', 'Commission calculation formula for ' + $scope.templateName + ' orders has been changed.<br>Do you want to save it?',
                function () {
                    $scope.saveFormula();
                    $("#formulaEditor").modal('hide');
                },
                function () {
                    $("#formulaEditor").modal('hide');
                }
            );
        }
        else {
            $("#formulaEditor").modal('hide');
        }
    }

    showFormulaTestError = function (errorMessage) {
        $scope.test.Output.Error = errorMessage;
        $timeout(function () { $scope.$apply(); });
    }

    $scope.savePayrollDate = function () {
        $scope.tmpOrder.Status = $scope.statuses.paid;
        $scope.tmpOrder.PaidBy = $scope.user.Name;
        $scope.tmpOrder.PayDate = new Date();
        $scope.tmpOrder.PayrollDate = $scope.payrollDate;
        $("#editPayrollDate").modal('hide');
        notifySuccess("Order " + $scope.tmpOrder.PONumber + " is marked as paid.", 2000);
        $scope.saveOrder();
    }

    normalizeOrder = function (order) {
        //if (order.OrderDate) order.OrderDate = $.datepicker.formatDate('mm/dd/y', parseJsonDate(order.OrderDate));
        if (order.OrderDate)
            order.OrderDate = parseJsonDate(order.OrderDate);
        //else
        //    showError("OrderDate undefined.");
        if (order.EstimatedShipDate) order.EstimatedShipDate = parseJsonDate(order.EstimatedShipDate);
        if (order.FinalPaymentDate) order.FinalPaymentDate = parseJsonDate(order.FinalPaymentDate);
        if (order.ApprovedDate) order.ApprovedDate = parseJsonDate(order.ApprovedDate);
        if (order.ReleasedDate) order.ReleasedDate = parseJsonDate(order.ReleasedDate);
        if (order.PayDate) order.PayDate = parseJsonDate(order.PayDate);
        if (order.PayrollDate) order.PayrollDate = parseJsonDate(order.PayrollDate);
        return order;
    }

    $scope.exportToCSV = function () {
        $scope.orderGrid.export('csv');
    }

    $scope.exportToPDF = function () {
        $scope.orderGrid.export('pdf');
    }

    $scope.exportOrderToPDF = function (isFullInfo) {
        orderToPDF($scope, isFullInfo);
    }
}]);
