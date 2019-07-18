function OrderGrid(scope, gridID, onGridRowDouldClicked) {
    this.scope = scope;
    this.gridID = gridID;
    this.data = [];
    var self = this;

    var selectedRowIndex = 0;

    getColumnNames = function () {
        var columnNames = ['', ''];

        $.each(self.scope.columns, function (index, column) {
            columnNames.push(self.scope.getColumnHeaderFor(column));
        });

        return columnNames;
    };

    getColumnFor = function (columnName) {
        switch (columnName) {
            case 'PONumber':
                return { name: 'PONumber', align: 'center', width: 75, editable: false, autoResizing: { minColWidth: 50 }, searchoptions: { clearSearch: true } };
            case 'SalesOrder':
                return { name: 'SalesOrder', align: 'center', width: 70, editable: false, autoResizing: { minColWidth: 50 }, searchoptions: { clearSearch: true } };
            case 'Template':
                return { name: 'Template', width: 90, editable: false, autoResizing: { minColWidth: 70 }, searchoptions: { clearSearch: true } };
            case 'Customer':
                return { name: 'Customer', width: 140, editable: false, autoResizing: { minColWidth: 100 }, searchoptions: { clearSearch: true } };
            case 'OrderDate':
                return { name: 'OrderDate', width: 80, editable: false, autoResizing: { minColWidth: 50 }, align: 'center', sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'm/d/y' }, search: false };
            case 'EstimatedShipDate':
                return { name: 'EstimatedShipDate', width: 80, editable: false, autoResizing: { minColWidth: 60 }, align: 'center', sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'm/d/y' }, search: false };
            case 'FinalPaymentDate':
                return { name: 'FinalPaymentDate', width: 80, editable: false, autoResizing: { minColWidth: 60 }, align: 'center', sorttype: 'date', formatter: 'date', formatoptions: { newformat: 'm/d/y' }, search: false };
            case 'TotalListPrice':
                return {
                    name: 'TotalListPrice', width: 110, editable: false, align: 'right', autoResizing: { minColWidth: 100 }, sorttype: 'float', formatter: 'currency', search: false,
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }
                };
            case 'SalePrice':
                return {
                    name: 'SalePrice', width: 120, editable: false, align: 'right', autoResizing: { minColWidth: 100 }, sorttype: 'float', formatter: 'currency', search: false,
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }
                };
            case 'DiscountAmount':
                return {
                    name: 'DiscountAmount', width: 100, editable: false, align: 'right', autoResizing: { minColWidth: 70 }, sorttype: 'float', formatter: 'currency', search: false,
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }
                };
            case 'DiscountPercent':
                return {
                    name: 'DiscountPercent', width: 65, editable: false, align: 'right', autoResizing: { minColWidth: 50 }, sorttype: 'float', formatter: 'number', search: false,
                    formatoptions: { prefix: '', suffix: '%', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }
                };
            case 'SalesmanName':
                return { name: 'SalesmanName', width: 180, editable: false, align: 'center', autoResizing: { minColWidth: 140 }, searchoptions: { clearSearch: true } };
            case 'RegionalManagerName':
                return { name: 'RegionalManagerName', width: 120, editable: false, align: 'center', autoResizing: { minColWidth: 80 }, searchoptions: { clearSearch: true } };
            case 'CostCenter':
                return { name: 'CostCenter', width: 80, editable: false, align: 'center', autoResizing: { minColWidth: 60 }, searchoptions: { clearSearch: true } };
            case 'RSMCostCenter':
                return { name: 'RSMCostCenter', width: 80, editable: false, align: 'center', autoResizing: { minColWidth: 60 }, searchoptions: { clearSearch: true } };
            case 'DealerCommissionInfo':
                return {
                    name: 'DealerCommissionInfo', width: 120, editable: false, align: 'right', autoResizing: { minColWidth: 80 }, sorttype: 'float', search: false,
                    hidden: self.scope.user.Role == self.scope.roles.pm,
                    cellattr: function (rowId, cellValue, rowObject) {
                        if (self.scope.MajorVersion > 1 && rowObject.IsDealerCommissionOverriden)
                            return 'title = "Calculated\nDealer/DSE\nCommission:\n$' + rowObject.CalculatedDealerCommission + '"';
                        else
                            return '';
                    }
                };
            case 'RegionalManagerCommission':
                return {
                    name: 'RegionalManagerCommission', width: 100, editable: false, align: 'right', autoResizing: { minColWidth: 80 }, sorttype: 'float', formatter: 'currency', searchoptions: { sopt: ['eq'] },
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }, search: false,
                    hidden: self.scope.user.Role == self.scope.roles.dealer || self.scope.user.Role == self.scope.roles.dse || self.scope.user.Role == self.scope.roles.pm,
                    cellattr: function (rowId, cellValue, rowObject) {
                        if (self.scope.MajorVersion > 1 && rowObject.IsRSMCommissionOverriden)
                            return 'title = "Calculated\nRSM\nCommission:\n$' + rowObject.CalculatedRegionalManagerCommission + '"';
                        else
                            return '';
                    }
                };
            case 'ProductManagerCommission':
                return {
                    name: 'ProductManagerCommission', width: 100, editable: false, align: 'right', autoResizing: { minColWidth: 80 }, sorttype: 'float', formatter: 'currency', searchoptions: { sopt: ['eq'] },
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' }, search: false,
                    hidden: self.scope.user.Role == self.scope.roles.dealer || self.scope.user.Role == self.scope.roles.dse || self.scope.user.Role == self.scope.roles.rsm,
                    cellattr: function (rowId, cellValue, rowObject) {
                        if (self.scope.MajorVersion > 1 && rowObject.IsPMCommissionOverriden)
                            return 'title = "Calculated\nProduct Manager\nCommission:\n$' + rowObject.CalculatedProductManagerCommission + '"';
                        else
                            return '';
                    }
                };
            case 'Status':
                return { name: 'Status', width: 80, editable: false, align: 'center', autoResizing: { minColWidth: 60 }, searchoptions: { clearSearch: true } };
            case 'ApprovedInfo':
                return { name: 'ApprovedInfo', width: 95, align: 'center', editable: false, autoResizing: { minColWidth: 80 }, search: false};
            case 'ReleasedInfo':
                return { name: 'ReleasedInfo', width: 95, align: 'center', editable: false, autoResizing: { minColWidth: 80 }, search: false };
            case 'PaidInfo':
                return { name: 'PaidInfo', width: 95, align: 'center', editable: false, autoResizing: { minColWidth: 80 }, search: false };
            case 'Comment':
                return { name: 'Comment', width: 95, align: 'left', editable: false, autoResizing: { minColWidth: 80 } };
            case 'YtdSale':
                return {
                    name: 'YtdSale', width: 95, align: 'center', editable: false, autoResizing: { minColWidth: 80 }, sorttype: 'float', formatter: 'currency', search: false,
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' } };
            case 'SaleGoal':
                return {
                    name: 'SaleGoal', width: 95, align: 'center', editable: false, autoResizing: { minColWidth: 80 }, sorttype: 'float', formatter: 'currency', search: false,
                    formatoptions: { prefix: '$', suffix: '', thousandsSeparator: ',', decimalPlaces: 2, defaultValue: '' } };
            default:
                return '';
        }
    };

    getColumns = function () {
        var columnNames =
        [
            {
                name: 'ID', width: 16, fixed: true, sortable: false, search: false, hidden: false, hidden: self.scope.user.Role != self.scope.roles.administrator,
                formatter: function (cellvalue, options, rowObject) {
                    var color = rowObject.Status == 'pending' ? 'color: #e42820' : 'color: transparent';
                    return "<span style='" + color + "' class='glyphicon glyphicon-refresh'></span>"
                },
                cellattr: function (rowId, cellValue, rowObject) {
                    return 'title="Recalculate Order"';
                }
            },
            {
                name: 'ID1', width: 16, fixed: true, sortable: false, search: false, hidden: !self.scope.permissions.CanCreateOrder,
                formatter: function (cellvalue, options, rowObject) {
                    var color = rowObject.Status == 'pending' ? 'color: #e42820' : 'color: transparent';
                    return "<span style='" + color + "' class='glyphicon glyphicon-remove'></span>"
                },
                cellattr: function (rowId, cellValue, rowObject) {
                    return 'title="Delete Order"';
                }
            }
        ];

        $.each(self.scope.columns, function (index, column) {
            columnNames.push(getColumnFor(column));
        });

        return columnNames;
    };

    //scope.$watch("showActiveTasksOnly", function (newValue, oldValue) {
    //    self.updateSelectedTask(scope.tasks, scope.task);
    //});

    $(this.gridID).jqGrid({
        colNames: getColumnNames(),
        colModel: getColumns(),
        data: self.data,
        datatype: "local",
        height: "auto",
        //shrinkToFit: true,
        //autowidth: false,
        //width: 900,
        width: $(document).width(),
        rowNum: 1000,
        rownumbers: true,
        hoverrows: true,
        viewrecords: true,
        caption: null,
        hidegrid: false,
        sortname: 'OrderDate',
        sortorder: "desc",
        multiselect: false,
        cmTemplate: { title: false },
        footerrow: true,
        userDataOnFooter: true,
        headertitles:true,
        sortable: {
            options: { // let reorder all columns, except with names ID, ID1.
                items: ">th:not(:has(#jqgh_order_grid_cb,#jqgh_order_grid_ID,#jqgh_order_grid_ID1,#jqgh_order_grid_rn,#jqgh_order_grid_subgrid),:hidden)"
            },
            update: function (relativeColumnOrder) {
                var columnNames = $(self.gridID).jqGrid("getGridParam", "colNames");
                var columns = [];
                $.each(columnNames, function (index, columnName) {
                    if (columnName)
                        columns.push(self.scope.getColumnNameFor(columnName));
                });
                self.scope.columns = columns;
                self.scope.saveColumnsAfterDragAndDrop();
            }
        },
        loadComplete: function () {
            var rowIds = $(this).jqGrid('getDataIDs');
            //if (rowIds.length === 0) return;
            if (rowIds.length > selectedRowIndex) 
                $(this).setSelection(rowIds[selectedRowIndex]);

            self.totalListPrice = 0;
            self.totalSellingPrice = 0;
            self.totalDealerCommission = 0;
            self.totalDiscount = 0;
            self.totalRegionalManagerCommission = 0;
            self.totalProductManagerCommission = 0;

            // highlight overriden commission values
            $.each(self.data, function (i, item) {
                var order = jQuery(self.gridID).jqGrid("getLocalRow", rowIds[i]);
                if (order.IsDealerCommissionOverriden && order.DealerCommissionInfo) 
                    jQuery(self.gridID).setCell(rowIds[i], 'DealerCommissionInfo', '', { 'background-color': '#ddeedd' });
                if (order.IsRSMCommissionOverriden && order.RegionalManagerCommission) 
                    jQuery(self.gridID).setCell(rowIds[i], 'RegionalManagerCommission', '', { 'background-color': '#ddeedd' });
                if (order.IsPMCommissionOverriden && order.ProductManagerCommission) 
                    jQuery(self.gridID).setCell(rowIds[i], 'ProductManagerCommission', '', { 'background-color': '#ddeedd' });

                if (typeof order.TotalListPrice === 'number')
                    self.totalListPrice += order.TotalListPrice;
                if (typeof order.SalePrice === 'number')
                    self.totalSellingPrice += order.SalePrice;
                if (order.DealerCommissionInfo)
                    self.totalDealerCommission += order.DealerCommission;
                if (typeof order.DiscountAmount === 'number')
                    self.totalDiscount += order.DiscountAmount;
                if (typeof order.RegionalManagerCommission === 'number')
                    self.totalRegionalManagerCommission += order.RegionalManagerCommission;
                if (typeof order.ProductManagerCommission === 'number')
                    self.totalProductManagerCommission += order.ProductManagerCommission;
            });

            $(this).jqGrid('footerData', 'set', { TotalListPrice: self.totalListPrice });
            $(this).jqGrid('footerData', 'set', { SalePrice: self.totalSellingPrice });
            $(this).jqGrid('footerData', 'set', { DiscountAmount: self.totalDiscount });
            $(this).jqGrid('footerData', 'set', { DealerCommissionInfo: '$' + self.totalDealerCommission.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,') });
            $(this).jqGrid('footerData', 'set', { RegionalManagerCommission: self.totalRegionalManagerCommission });
            $(this).jqGrid('footerData', 'set', { ProductManagerCommission: self.totalProductManagerCommission });

            //$(this).jqGrid('setGridParam', {
            //    userData: {
            //        'Customer': 'Total', 'TotalListPrice': self.totalListPrice, 'SalePrice': self.totalSellingPrice, 'DiscountAmount': self.totalDiscount,
            //        'DealerCommissionInfo': self.totalDealerCommission, 'RegionalManagerCommission': RegionalManagerCommission, 'ProductManagerCommission': self.totalProductManagerCommission
            //    }
            //});
        },
        ondblClickRow: function (rowId) {
            $(this).setSelection(rowId);
            var order = $(this).jqGrid("getLocalRow", rowId);
            onGridRowDouldClicked(order.ID);
        },
        beforeSelectRow: function (rowId, e) {
            var idArray = $(this).jqGrid('getDataIDs');
            var selectedRowID = $(this).jqGrid("getGridParam", "selrow");
            var iCol = $.jgrid.getCellIndex($(e.target).closest("td")[0]);

            var order = $(this).jqGrid("getLocalRow", rowId);
            if (iCol == 1 && order.Status == 'pending') {
                self.scope.recalculateOrder(order);
            }
            if (iCol == 2 && order.Status == 'pending') {
                showYesNoMessage('Orders', 'Do you want to delete order ' + order.Template + ' / ' + order.PONumber + '?',
                    function () {
                        self.scope.deleteOrder(order);
                    }
                );
            }

            if (selectedRowID == rowId) {
                return false;
            }

            selectedRowID = rowId;
            selectedRowIndex = _.findIndex(idArray, function (id) { return id == selectedRowID; });
            return true;
        }
    });

    //jQuery(this.gridID).jqGrid('setGroupHeaders', {
    //    useColSpanStyle: true,
    //    groupHeaders: [
    //        { startColumnName: 'TotalListPrice', numberOfColumns: 4, titleText: '<em style="color:black">Price</em>' },
    //        { startColumnName: 'DealerCommission', numberOfColumns: 3, titleText: '<em style="color:black">Commission</em>' },
    //        { startColumnName: 'Status', numberOfColumns: 4, titleText: '<em style="color:black">Status</em>' }
    //    ]
    //});

    jQuery(this.gridID).jqGrid('filterToolbar', { searchOnEnter: false, stringResult: true, defaultSearch:'cn' });

    this.filterData = function (data, filter) {
        var filteredData = [];

        $.each(data, function (index, order) {
            var customerName = _.find(self.scope.customers, function (c) { return c.ID == order.CustomerID; }).Name;
            var salesman = _.find(self.scope.salesmen, function (s) { return s.ID.toLowerCase() == order.Salesman.toLowerCase(); });
            var salesman2 = (order.Salesman2 == null  || order.Salesman2 == scope.Salesman2_NA.ID) ? null : _.find(self.scope.salesmen, function (s) { return s.ID.toLowerCase() == order.Salesman2.toLowerCase(); });
            var rsm = (order.RSM == null || order.RSM == scope.Salesman2_NA.ID) ? null : _.find(self.scope.salesmen, function (s) { return s.ID.toLowerCase() == order.RSM.toLowerCase(); });

            var template = _.find(self.scope.templates, function (t) { return t.ID == order.TemplateID; });
            var _order = jQuery.extend(true, {}, order); // deep copy
            _order.Customer = customerName;
            _order.SalesmanName = salesman.Name + ' (' + salesman.Region + ')';
            if (salesman2) _order.SalesmanName += '<br>' + salesman2.Name + ' (' + salesman2.Region + ')';
            _order.RegionalManagerName = rsm ? rsm.Name : null;
            if (!_order.PayRSMCommission)
                _order.RegionalManagerCommission = null; 
            _order.Template = template.Name;
            _order.Region = salesman.Region;
            _order.CostCenter = salesman.CostCenter;
            _order.RSMCostCenter = rsm ? rsm.CostCenter : null;

            if (salesman.Role == self.scope.roles.rsmdse) {
                _order.DealerCommissionInfo = '';
            }
            else {
                if (salesman2) {
                    var dealerCommision = _order.DealerCommission / 2;
                    _order.DealerCommissionInfo = self.formatCurrency(dealerCommision) + '<br>' + self.formatCurrency(dealerCommision);
                }
                else {
                    _order.DealerCommissionInfo = self.formatCurrency(_order.DealerCommission);
                }
            }

            _order.ApprovedInfo = _order.ApprovedBy ? _order.ApprovedBy + '<br>' + $.datepicker.formatDate('mm/dd/y', _order.ApprovedDate) : '---';
            _order.ReleasedInfo = _order.ReleasedBy ? _order.ReleasedBy + '<br>' + $.datepicker.formatDate('mm/dd/y', _order.ReleasedDate) : '---';
            _order.PaidInfo = _order.PaidBy ? _order.PaidBy + '<br>' + $.datepicker.formatDate('mm/dd/y', _order.PayrollDate) : '---';

            if (!template.ProductManager) _order.ProductManagerCommission = ''; 

            //var approvedBy = _.find(self.scope.salesmen, function (s) { return s.ID == order.ApprovedBy; });
            //_order.ApprovedBy = approvedBy ? approvedBy.Name : '';
            //var releasedBy = _.find(self.scope.salesmen, function (s) { return s.ID == order.ReleasedBy; });
            //_order.ReleasedBy = releasedBy ? releasedBy.Name : '';

            //_order.DealerCommission = _.find(_order.OutputData.OrderItems, function (item) { return item.Variable == 'dealer_comission'; }).Value;
            //_order.Discount = _.find(_order.OutputData.OrderItems, function (item) { return item.Variable == 'discount_amount'; }).Value;
            ////_order.DiscountPercent = _.find(_order.OutputData.OrderItems, function (item) { return item.Variable == 'discount_percent'; }).Value;
            //_order.RegionalManagerCommission = _.find(_order.OutputData.OrderItems, function (item) { return item.Variable == 'regional_manager_comission'; }).Value;
            //_order.ProductManagerCommission = _.find(_order.OutputData.OrderItems, function (item) { return item.Variable == 'product_manager_comission'; }).Value;

            _order.YtdSale = _order.YtdSaleBeforeThisOrder + _order.SalePrice;
            let orderYear = _order.OrderDate ? _order.OrderDate.getFullYear() : new Date().getFullYear();
            _order.SaleGoal = 0;
            if (salesman) {
                let goal = _.find(salesman.SalesGoals, function (s) { return s.Year === orderYear; });
                _order.SaleGoal = goal ? goal.Target : 0;
            }




            if (self.matchOrder(_order, filter)) { 
                filteredData.push(_order);
            }
        });
        return {
            tableData: filteredData 
        };
    }

    this.formatCurrency = function (value) {
        return '$' + value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
    }

    this.matchOrder = function (order, filter) {
        var matches = true;
        if (filter) {
            if (filter.Status) {
                let isFound = false;
                $.each(filter.Status, function (index, status) {
                    if (order.Status == status) {
                        isFound = true;
                        return;
                    }
                });
                matches &= isFound;
            }
            if (filter.Region) {
                let isFound = false;
                $.each(filter.Region, function (index, region) {
                    if (order.Region == region) {
                        isFound = true;
                        return;
                    }
                });
                matches &= isFound;
            }
            if (filter.Salesman) {
                let isFound = false;
                $.each(filter.Salesman, function (index, salesman) {
                    if (order.Salesman == salesman || order.Salesman2 == salesman || order.RSM == salesman) {
                        isFound = true;
                        return;
                    }
                });
                matches &= isFound;
            }
            if (filter.Product) {
                let isFound = false;
                $.each(filter.Product, function (index, product) {
                    if (order.TemplateID == product) {
                        isFound = true;
                        return;
                    }
                });
                matches &= isFound;
            }
            // date range filter
            if (filter.StartDate)
                matches &= order.OrderDate >= new Date(filter.StartDate);
            if (filter.EndDate)
                matches &= order.OrderDate <= new Date(filter.EndDate);

            if (filter.ShipStartDate)
                matches &= order.EstimatedShipDate >= new Date(filter.ShipStartDate);
            if (filter.ShipEndDate)
                matches &= order.EstimatedShipDate <= new Date(filter.ShipEndDate);
        }

        return matches;
    }

    this.updateData = function (orders, filter) {
        var data = this.filterData(orders, filter);
        jQuery(this.gridID).jqGrid('clearGridData');
        jQuery(this.gridID).jqGrid('setGridParam', {
            data: data.tableData//,
            //userData: {
            //    'Customer': 'Total', 'TotalListPrice': this.totalListPrice, 'SalePrice': this.totalSellingPrice, 'DiscountAmount': this.totalDiscount,
            //    'DealerCommissionInfo': this.formatCurrency(this.totalDealerCommission), 'RegionalManagerCommission': this.totalRegionalManagerCommission, 'ProductManagerCommission': this.totalProductManagerCommission
            //}
        });
        jQuery(this.gridID).trigger('reloadGrid');
    }

    this.getData = function () {
        return jQuery(this.gridID).jqGrid('getGridParam', 'data');
    }

    this.export = function (format) {
        var data = $(gridID).jqGrid('getRowData');
        var totalData = $(gridID).jqGrid('footerData', 'get');
        var columnNames = $(gridID).jqGrid('getGridParam', 'colNames');
        $.each(data, function (index, row) {
            delete row['ID'];
            delete row['ID1'];
        });
        if (format == 'csv')
            CSVExport(columnNames, data, (self.scope.IsDemoVersion ? '' : 'Bystronic') + 'Orders.csv');
        else
            orderTableToPDF(self.scope, columnNames, data, totalData, (self.scope.IsDemoVersion ? '' : 'Bystronic') + 'Orders.pdf');
    }
}
