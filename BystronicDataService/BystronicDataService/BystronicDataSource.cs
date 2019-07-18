using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BystronicDataService
{
    class BystronicDataSource : DataSource
    {
        public override bool IsReady()
        {
            return true;
        }

        public override List<User> GetAllSalesmen()
        {
            List<User> toret = new List<User>();
              BystronicDataDataContext bd = new BystronicDataDataContext();
            foreach (SalesPeopleList p in bd.SalesPeopleLists.AsEnumerable())
            {
                toret.Add(new User(p.ADID,(p.FirstName+ " " + p.LastName).TrimEnd(), p.Type, p.Region, p.CostCenter, (double)p.YTDSales,p.SalesGoals));
            }
            return toret;
            /*
            return new List<Salesman>()
            {
                new Salesman("Admin",   "password", "Administrator",    Salesman.ROLE_ADMINISTRATOR,    "Midwest"),
                new Salesman("Marina",  "password", "Marina Dubovis",   Salesman.ROLE_SALESMAN,         "Northeast"),
                new Salesman("Ilya",    "password", "Ilya Dubovis",     Salesman.ROLE_SALESMAN,         "Midwest"),
                new Salesman("Chris",   "password", "Chris Smith",      Salesman.ROLE_SALESMAN,         "Northeast"),
                new Salesman("Luba",    "password", "Luba Dubovis",     Salesman.ROLE_APPROVER,         "Midwest"),
                new Salesman("Billy",   "password", "Billy Cabrales",   Salesman.ROLE_REGIONAL_MANAGER, "Midwest")
            };*/
        }

        public override List<Product> GetProducts()
        {
            List<Product> toret = new List<Product>();
            BystronicDataDataContext bd = new BystronicDataDataContext();

            foreach (ProductList p in bd.ProductLists.AsEnumerable())
            {
                toret.Add(new Product(p.ProductID, p.ProductTypeID.Value, p.Name, (double)p.Price, p.Tooling.HasValue && p.Tooling.Value));
            }

            return toret;

            /*
            return new List<Product>()
            {
                new Product(111, "BySprint Fiber 3015 1kw", 739000.00),
                new Product(112, "BySprint Fiber 3015 2kw", 839000.00),
                new Product(113, "BySprint Fiber 3015 6kw", 939000.00),
                new Product(114, "BySprint Fiber 3015 9kw", 1299000.00),
                new Product(115, "Retrofit into existing automation", 12000.00),
                new Product(116, "2nd year warranty", 10100.00),
                new Product(117, "3rd year warranty", 11100.00),
                new Product(118, "4th year warranty", 11200.00),
                new Product(119, "Observer Hardware"),
                new Product(120, "Trust pack fiber warranty"),
                new Product(121, "Operator training course 941-2 people Elgin")
            };*/
        }

        public override List<Customer> GetCustomers()
        {
            List<Customer> toret = new List<Customer>();
            BystronicDataDataContext bd = new BystronicDataDataContext();
           
            foreach (CustomerList p in bd.CustomerLists.AsEnumerable())
            {
                toret.Add(new Customer(p.CustomerID, p.Name, p.Address, p.City, p.State, p.Zip, p.SAPNumber)); // add ZipCode parameter
            }
            return toret;
            /*
            return new List<Customer>()
            {
                new Customer(211, "Julien, Inc.", "Info 111"),
                new Customer(212, "Prince Fabricators", "Info 222"),
                new Customer(213, "ABC Manufacture, LLC.", "Info 333"),
                new Customer(214, "XYZ and Company, Inc.", "Info 444")
            };*/
        }

        //public Order(int id, int jobNumber, int typeOfSale, int templateID, string status, string salesman, DateTime orderDate,
        //             int customerID, string poNumber, string trackingNumber,
        //             string approvedBy, DateTime? approvedDate, string releasedBy, DateTime? releasedDate, string comment,
        //             List<OrderItem> orderItems,
        //             double totalListPrice, double salePrice, double discountAmount, double discountPercent, double dealerCommission, double regionalManagerCommission, double productManagerCommission)

        public override List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();
            BystronicDataDataContext bd = new BystronicDataDataContext();

            foreach (OrderList p in bd.OrderLists.AsEnumerable())
            {
                List<OrderItem> oiList = new List<OrderItem>();
                foreach (OrderItemList oil in bd.OrderItemLists.Where(e => e.OrderID == p.OrderID))
                {
                    var listPrice = oil.ListPrice.HasValue ? (double)oil.ListPrice : 0.0;
                    var toolingPrice = oil.ToolingPrice.HasValue ? (double)oil.ToolingPrice : 0.0;
                    oiList.Add(new OrderItem(oil.ProductID, listPrice, toolingPrice));
                }
                orders.Add(new Order(
                    p.OrderID,
                    p.OrderID,
                    p.TemplateID.Value,
                    p.Status,p.DateOfSale.Value,
                    p.SalesPersonADID,
                    p.SalesPerson2ADID,
                    // Add RSMADID here !!!
                    p.RSMADID,
                    p.PayRSMCommission.HasValue ? p.PayRSMCommission.Value : true,
                    p.CustomerID.Value,
                    p.PONumber,
                    p.TrackingNumber,
                    p.ApprovedBy,
                    p.ApprovedDate,
                    p.ReleasedBy,
                    p.ReleasedDate,
                    p.PaidBy,
                    p.PaidDate,
                    p.PayrollDate,
                    p.Comments,
                    oiList,
                    (double)p.TotalProductCost,
                    (double)p.SalePrice,
                    (double)p.DiscountAmount,
                    (double)p.DiscountPerc,
                    (double)p.DealerCommission,
                    (double)p.RMCommission,
                    (double)p.PMCommission,
                p.SalesOrder,
                p.EstimatedShipDate,
                p.FinalPaymentDate
                    ));

                
                
            }
            
            /*
            var orders = new  List<Order>()
            {
                new Order(111,          // ID
                          3544,         // JobNumber
                          0,            // Type of sale (0-dealer; 1-direct)
                          911,          // templete ID
                          "pending",    // status
                          "Ilya",       // saleman's login
                          DateTime.Parse("02/03/2017"), // order date
                          211,          // customer ID
                          "QUO-35730-Rev 2 SO", // P.O. Number
                          "5723576761", // tracking number
                          null,         // approved by (full name)
                          null,         // approved date
                          null,         // released by (full name)
                          null,         // released date
                          null,         // comment
                          new List<OrderItem> ()
                          {
                              new OrderItem(113, 910000.00),
                              new OrderItem(117, 11000.00),
                          },
                          0.0,
                          900000.00,
                          0.0,
                          0.0,
                          0.0,
                          0.0,
                          0.0
                ),
                new Order(112,          // ID
                          3589,         // JobNumber
                          1,            // Type of sale (0-dealer; 1-direct)
                          912,          // templete ID
                          "Approved",    // status
                          "Ilya",       // saleman's id
                          DateTime.Parse("05/08/2017"), // order date
                          212,          // customer ID
                          "RRT-45723-Rev 1 SO", // P.O. Number
                          "8736452187", // tracking number
                          "Luba Dubovis",         // approved by (name)
                          DateTime.Parse("06/25/17"),         // approved date
                          null,         // released by
                          null,         // released date
                          null,         // comment
                          new List<OrderItem> ()
                          {
                              new OrderItem(112, 835000.00),
                              new OrderItem(115, 11500.00),
                          },
                          0.0,
                          812000.00,
                          0.0,
                          0.0,
                          0.0,
                          0.0,
                          0.0
                )
            };

            //    new Order(112, 3589, 0, 113, "approved", DateTime.Parse("09/05/2016"),  212, "RRT-45723-Rev 1 SO", "8736452187", "Ilya", "Luba Dubovis", DateTime.Parse("09/25/16"), null, null, null, null, "", inputData2),
            //    new Order(113, 3678, 1, 111, "pending", 113, DateTime.Parse("05/03/2017"), "ABC-23456-SO", "9875645235", 213, "Chris", null, null, null, null, null, null,  "", inputData1),
            //    new Order(114,  3987, 0, 112, 113, DateTime.Parse("03/13/2017"), "EFG-232323-FO", "12125235", 213, "Marina", null, null, null, null, null, null, "", inputData1)
            //};
            */


            //foreach (var order in orders)
            //    if (order.TotalListPrice == 0)
            //        order.Calculate();

            return orders;
        }

        public override bool AddOrder(Order order)
        {
            //add new order: ignore order.ID
            BystronicDataDataContext bd = new BystronicDataDataContext();
            order.ID = bd.AddOrder(DateTime.Now, 
                order.OrderDate, 
                order.Salesman, 
                order.Salesman2,
                order.RSM,
                order.PayRSMCommission,
                order.CustomerID, 
                order.PONumber, 
                (decimal)order.TotalListPrice, 
                (decimal)order.SalePrice, 
                (decimal)order.DiscountAmount, 
                (decimal)order.DiscountPercent, 
                (decimal)order.DealerCommission,
                (decimal)order.RegionalManagerCommission, 
                (decimal)order.ProductManagerCommission, 
                order.Comment, 
                order.Status, 
                order.ApprovedBy, 
                order.ApprovedDate, 
                order.ReleasedBy, 
                order.ReleasedDate,
                order.PaidBy,
                order.PayDate,
                order.PayrollDate,
                order.TrackingNumber, 
                order.TemplateID,
                order.SalesOrder,
                order.EstimatedShipDate,
                order.FinalPaymentDate);

            foreach (OrderItem item in order.OrderItems)
            {
                bd.AddOrderItem(order.ID, DateTime.Now, item.ProductID, (decimal)item.Value, (decimal) item.ToolingValue);
            }
            bd.SubmitChanges();

            return true;
        }

        public override bool UpdateOrder(Order order)
        {
            //update order in the database with the order.ID with given order
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.UpdateOrder(
               //order.orderID  I need order id
                order.ID,
                DateTime.Now,
                order.OrderDate,
                order.Salesman,
                order.Salesman2,
                order.RSM,
                order.PayRSMCommission,
                order.CustomerID, 
                order.PONumber,
                (decimal)order.TotalListPrice,
                (decimal)order.SalePrice,
                (decimal)order.DiscountAmount,
                (decimal)order.DiscountPercent,
                (decimal)order.DealerCommission,
                (decimal)order.RegionalManagerCommission,
                (decimal)order.ProductManagerCommission,
                order.Comment,
                order.Status,
                order.ApprovedBy,
                order.ApprovedDate,
                order.ReleasedBy,
                order.ReleasedDate,
                order.PaidBy,
                order.PayDate,
                order.PayrollDate,
                order.TrackingNumber,
                order.TemplateID, 
                order.SalesOrder,
                order.EstimatedShipDate,
                order.FinalPaymentDate);

            //should we update or delete and reinsert?
            bd.DeleteOrderItemsByOrderID(order.ID);
            foreach (OrderItem item in order.OrderItems)
            {
                bd.AddOrderItem(order.ID, DateTime.Now, item.ProductID, (decimal)item.Value, (decimal)item.ToolingValue);
            }
            bd.SubmitChanges();
            return true;
        }

        public override List<OrderTemplate> GetOrderTemplates()
        {
            List<OrderTemplate> orderTemplate = new List<OrderTemplate>();
            BystronicDataDataContext bd = new BystronicDataDataContext();
            foreach(Template t in bd.Templates)
            {
                orderTemplate.Add(new OrderTemplate(t.TemplateID, t.Name, t.Tooling.Value, t.PMADID));
            }
            return orderTemplate;
        }

        public override bool AddCustomer(Customer customer)
        {
            // Add code here
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.AddCustomer(customer.Name, customer.Address, customer.City, customer.State, customer.ZipCode, customer.SapNumber);
            return true;
        }

        public override bool DeleteOrder(int orderID)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.DeleteOrderByOrderID(orderID);
            return true;
        }

        public override bool UpdateCustomer(Customer customer)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            //1 is a region id, i have region id in db but the client doesn't need it for now
            bd.UpdateCustomer(customer.ID,customer.Name,customer.Address,customer.City, customer.State,customer.ZipCode,1, customer.SapNumber);
            return true;
        }

        public override bool DeleteCustomer(Customer customer)
        {
            //delete customer only if there are no orders for this customer otherwise we need to delete orders. Maybe a message to use "Please delete orders for this customer before deleting customer";
            BystronicDataDataContext bd = new BystronicDataDataContext();
           
            DeleteCustomerWOOrderResult result = bd.DeleteCustomerWOOrder(customer.ID).FirstOrDefault();
           
            //returns true if deleted, returns false if orders with customer id already exists
            return result.Column1.Value;
        }

        public override List<ProductType> GetProductTypes()
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            var type = from g in bd.ProductTypeLists select new ProductType(g.ProductTypeID, g.Type);
            return type.ToList();
            
        }

        public override bool AddProduct(Product product)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            //need to add product manager id here
            bd.AddProduct(product.ProductTypeID, product.Name, (decimal)product.Value, product.HasTooling, "productmanagerid");
            return true;
        }

        public override bool UpdateProduct(Product product)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.UpdateProduct(product.ID,product.ProductTypeID, product.Name, (decimal)product.Value, product.HasTooling, "productmanagerid");
            return true;
        }

        public override bool DeleteProduct(Product product)
        {
            //delete product only if there arent orders with this product
            BystronicDataDataContext bd = new BystronicDataDataContext();

            DeleteProductWOOrderResult result = bd.DeleteProductWOOrder(product.ID).FirstOrDefault();

            //returns true if deleted, returns false if orders with customer id already exists
            return result.Column1.Value;
        }


	//////////////////////// NEW NEW NEW ///////////////////////////////////////////////////////////////////


	    public override string GetFormula(int templateID)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            var template = bd.Templates.FirstOrDefault(e => e.TemplateID == templateID);
            if (template != null && template.Formula != null)
                return template.Formula;
            else
                return GetDefaultFormula(templateID);
        }

        public override bool SaveFormula(int templateID, string formula)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            Template oldTemplate = bd.Templates.FirstOrDefault(e => e.TemplateID == templateID);
            oldTemplate.Formula = formula;
            bd.SubmitChanges();
            return true;
        }

        public override bool AddSalesman(User salesman)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.AddSalesman(salesman.ID, salesman.Name, "", "rep id optional?", salesman.Region, salesman.Role, 0, 0,salesman.CostCenter,salesman.SalesGoalsAsString());
            return true;
        }

        public override bool UpdateSalesman(string id, User salesman)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            UpdateSalesmanResult result = bd.UpdateSalesman(id,salesman.ID, salesman.Name, "", "rep id optional?", salesman.Region, salesman.Role, 0, 0,salesman.CostCenter, salesman.SalesGoalsAsString()).FirstOrDefault();
            return result.Column1.Value;   
        }

        public override bool DeleteSalesman(User salesman)
        {
            //delete salesman only if there arent orders with this salseman
            BystronicDataDataContext bd = new BystronicDataDataContext();

            DeleteSalesmanResult result = bd.DeleteSalesman(salesman.ID).FirstOrDefault();

            //returns true if deleted, returns false if orders with customer id already exists
            return result.Column1.Value;
        }

        public override double GetYtdSale(string salesmanID, int orderID, DateTime orderDate)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            GetYTDBySalesmanOrderDateResult result = bd.GetYTDBySalesmanOrderDate(salesmanID, orderID, orderDate).FirstOrDefault();
            return (double)result.YTD.Value;
        }

	    public override string GetColumns(string username, string gridname)
        {
            // !!! return comma-separated string with column names or string.Empty
            BystronicDataDataContext bd = new BystronicDataDataContext();
            GetgRIDcolumnsByUserResult result = bd.GetgRIDcolumnsByUser(username, gridname).FirstOrDefault();
            return result != null ? result.gridcolumns : string.Empty;
        }

        public override List<View> GetViews(string username)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            var views = from view in bd.GetViewsByUser(username) select new View(view.gridname.Trim(), view.gridcolumns.Trim());
            return views.ToList();
        }

        public override void SaveColumns(string username, string gridname, string newgridname, string columns)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.AddUpdateUsersGridColumns(username, gridname, newgridname, columns);
        }

        public override void DeleteColumns(string username, string gridname)
        {
            BystronicDataDataContext bd = new BystronicDataDataContext();
            bd.DeleteGridByUserAndGridName(username, gridname);
        }
    }
}
