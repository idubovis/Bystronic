using System;
using System.Collections.Generic;

namespace BystronicDataService
{
    class TestDataSource : DataSource
    {
        private static List<Order> _allOrders = null;

        public override bool IsReady()
        {
           //return DateTime.Now.Minute >= 25;
           return true;
        }

        public override List<Order> GetAllOrders()
        {
            if (_allOrders == null)
            {
                _allOrders = new List<Order>();

                //_allOrders = new List<Order>()
                //{
                //    new Order(111,          // ID
                //              3544,         // JobNumber
                //              0,            // Type of sale (0-dealer; 1-direct)
                //              1,          // templete ID
                //              "pending",    // status
                //              "Ilya",       // saleman's login
                //              DateTime.Parse("02/03/2017"), // order date
                //              211,          // customer ID
                //              "QUO-35730-Rev 2 SO", // P.O. Number
                //              "5723576761", // tracking number
                //              null,         // approved by (full name)
                //              null,         // approved date
                //              null,         // released by (full name)
                //              null,         // released date
                //              null,         // comment
                //              new List<OrderItem> ()
                //              {
                //                  new OrderItem(113, 910000.00),
                //                  new OrderItem(117, 11000.00),
                //              },
                //              0.0,
                //              900000.00,
                //              0.0,
                //              0.0,
                //              0.0,
                //              0.0,
                //              0.0
                //    ),
                //    new Order(112,          // ID
                //              3589,         // JobNumber
                //              1,            // Type of sale (0-dealer; 1-direct)
                //              2,          // templete ID
                //              "approved",    // status
                //              "Ilya",       // saleman's id
                //              DateTime.Parse("05/08/2017"), // order date
                //              211,          // customer ID
                //              "RRT-45723-Rev 1 SO", // P.O. Number
                //              "8736452187", // tracking number
                //              "Luba Dubovis",         // approved by (name)
                //              DateTime.Parse("06/25/17"),         // approved date
                //              null,         // released by
                //              null,         // released date
                //              null,         // comment
                //              new List<OrderItem> ()
                //              {
                //                  new OrderItem(112, 835000.00),
                //                  new OrderItem(115, 11500.00),
                //              },
                //              0.0,
                //              812000.00,
                //              0.0,
                //              0.0,
                //              0.0,
                //              0.0,
                //              0.0
                //    )
                //};
            }

            //    new Order(112, 3589, 0, 113, "approved", DateTime.Parse("09/05/2016"),  212, "RRT-45723-Rev 1 SO", "8736452187", "Ilya", "Luba Dubovis", DateTime.Parse("09/25/16"), null, null, null, null, "", inputData2),
            //    new Order(113, 3678, 1, 111, "pending", 113, DateTime.Parse("05/03/2017"), "ABC-23456-SO", "9875645235", 213, "Chris", null, null, null, null, null, null,  "", inputData1),
            //    new Order(114,  3987, 0, 112, 113, DateTime.Parse("03/13/2017"), "EFG-232323-FO", "12125235", 213, "Marina", null, null, null, null, null, null, "", inputData1)
            //};

            //foreach (var order in orders)
            //    if(order.TotalListPrice == 0)
            //        order.Calculate();

            return _allOrders;
        }

        public override bool AddOrder(Order order)
        {
            //add new order: ignore order.ID
            //_allOrders.Add(order);
            order.ID = new Random().Next(1, 1000);
            return true;
        }

        public override bool UpdateOrder(Order order)
        {
            //var index = _allOrders.FindIndex(x => x.ID == order.ID);
            //if (index >= 0)
            //    _allOrders[index] = order;


            return true;
        }

        public override bool DeleteOrder(int orderID)
        {
            //var order = _allOrders.Find(x => x.ID == orderID);
            //if (order != null)
            //{
            //    _allOrders.Remove(order);
            //    return true;
            //}
            return false;
        }

        public override List<OrderTemplate> GetOrderTemplates()
        {
            return new List<OrderTemplate>()
            {
                new OrderTemplate(1, "Laser", false, "pm1"),
                new OrderTemplate(2, "Automation", false, "pm2"),
                new OrderTemplate(3, "Pressbrake", true, "pm3")
            };
        }

        private static List<User> _salesmen;
        public override List<User> GetAllSalesmen()
        {
            if (_salesmen == null)
            {
                var dict = "2017,100;2018,2000;2020,3333";

                _salesmen = new List<User>()
                {
                    new User("admin",    "Administrator",    User.ROLE_ADMINISTRATOR,    "Midwest"),
                    new User("marina",   "Marina Dubovis",   User.ROLE_DEALER,         "Midwest"),
                    new User("ilya",    "Ilya Dubovis",     User.ROLE_DEALER,         "Midwest"),
                    new User("alex",    "Alex White",      User.ROLE_DSE,         "East", "", 1200000, "2017,1000000;2018,2000000"),
                    new User("anna",    "Anna Brown",      User.ROLE_DSE,         "East", "", 987654, "2017,4000000;2018,5555555"),
                    new User("luba",     "Luba Dubovis",     User.ROLE_ADMINISTRATOR,         "Midwest"),
                    new User("lilly",     "Lilly Dubovis",     User.ROLE_ADMINISTRATOR,         "Midwest"),
                    new User("bob",     "Bob Berkshire",     User.ROLE_ADMINISTRATOR,         "Midwest"),

                    

                    new User("billy",    "Billy Cabrales",   User.ROLE_REGIONAL_MANAGER_DSE, "Midwest", "", 0, dict),
                    new User("pete",     "Pete Kanatzar",   User.ROLE_REGIONAL_MANAGER, "East"),

                    new User("pm1",    "Product Manager 1",   User.ROLE_PRODUCT_MANAGER, "South"),
                    new User("pm2",    "Product Manager 2",   User.ROLE_PRODUCT_MANAGER, "South"),
                    new User("pm3",    "Product Manager 3",   User.ROLE_PRODUCT_MANAGER, "South")
                };
            }
            return _salesmen;
        }

        public override bool AddSalesman(User salesman)
        {
            _salesmen.Add(salesman);
            return true;
        }

        public override bool UpdateSalesman(string id, User salesman)
        {
            var index = _salesmen.FindIndex(s => s.ID == id);
            if (index >= 0)
            {
                _salesmen[index] = salesman;
                return true;
            }
            return false;
        }

        public override bool DeleteSalesman(User salesman)
        {
            var existingSalesman = _salesmen.Find(s => s.ID == salesman.ID);
            if (existingSalesman != null)
                return _salesmen.Remove(existingSalesman);
            return false;
        }

        private static List<Customer> _customers;

        public override List<Customer> GetCustomers()
        {
            if (_customers == null)
            {
                _customers = new List<Customer>()
                {
                    new Customer(211, "Julien, Inc.", "111 Main St.", "AnyTown", "IL", "60606", "111111111"),
                    new Customer(212, "Prince Fabricators", "222 Main St.", "AnyTown", "NY", "12606", "2222222222222"),
                    new Customer(213, "ABC Manufacture, LLC.","333 Main St.", "AnyTown", "FL", "22606", "3333333333"),
                    new Customer(214, "XYZ and Company, Inc.", "444 Main St.", "AnyTown", "BC", "33606", "44444444444444")
                };
            }
            return _customers;
        }

        public override bool AddCustomer(Customer customer)
        {
            // Add code here
            customer.ID = new Random(DateTime.Now.Millisecond).Next(210, 900);
            _customers.Add(customer);
            return true;
        }

        public override bool UpdateCustomer(Customer customer)
        {
            var index = _customers.FindIndex(c => c.ID == customer.ID);
            if (index >= 0)
            {
                _customers[index] = customer;
                return true;
            }
            return false;
        }

        public override bool DeleteCustomer(Customer customer)
        {
            var existingCustomer = _customers.Find(c => c.ID == customer.ID);
            if (existingCustomer != null)
                return _customers.Remove(existingCustomer);
            return false;
        }

        public override List<ProductType> GetProductTypes()
        {
            return new List<ProductType>()
            {
                new ProductType(1, "Laser"),
                new ProductType(2, "Automation"),
                new ProductType(3, "Pressbrake"),
                new ProductType(4, "Software"),
                new ProductType(5, "Services"),
                new ProductType(6, "Tube Laser"),
            };
        }

        private static List<Product> _products;

        public override List<Product> GetProducts()
        {
            if (_products == null)
            {
                _products = new List<Product>()
                {
                    new Product(111, 3, "BySprint Fiber 3015 1kw", 739000.00, true),
                    new Product(112, 1, "BySprint Fiber 3015 2kw", 839000.00, true),
                    new Product(113, 1, "BySprint Fiber 3015 6kw", 939000.00, true),
                    new Product(114, 2, "BySprint Fiber 3015 9kw", 1299000.00, true),
                    new Product(115, 1, "Retrofit into existing automation", 12000.00),
                    new Product(135, 3, "Xpert 400/200", 888000.00, true),
                    new Product(136, 3, "Xact 200", 993000.00, true),
                    new Product(116, 4, "2nd year warranty", 10100.00),
                    new Product(117, 4, "3rd year warranty", 11100.00),
                    new Product(118, 4, "4th year warranty", 11200.00),
                    new Product(119, 5, "Observer Hardware"),
                    new Product(120, 5, "Trust pack fiber warranty"),
                    new Product(121, 5, "Operator training course 941-2 people Elgin")
                };
            }
            return _products;
        }

        public override bool AddProduct(Product product)
        {
            _products.Add(product);
            return true;
        }

        public override bool UpdateProduct(Product product)
        {
            var index = _products.FindIndex(p => p.ID == product.ID);
            if (index >= 0)
            {
                _products[index] = product;
                return true;
            }
            return false;
        }

        public override bool DeleteProduct(Product product)
        {
            var existingProduct = _products.Find(p => p.ID == product.ID);
            if (existingProduct != null)
                return _products.Remove(existingProduct);
            return false;
        }

        public static Dictionary<int, string> _formulas = new Dictionary<int, string>();

        public override string GetFormula(int templateID)
        {
            if (!_formulas.ContainsKey(templateID))
                _formulas[templateID] = GetDefaultFormula(templateID);
            return _formulas[templateID];
        }

        public override bool SaveFormula(int templateID, string formula)
        {
            _formulas[templateID] = formula;
            return true;
        }

        public override double GetYtdSale(string salesmanID, int orderID, DateTime orderDate)
        {
            // return 3000000; 
            return 3932027.00;
        }

        private Dictionary<string, string> _columns = new Dictionary<string, string>();

        public override string GetColumns(string username, string gridname = "Default")
        {
            return _columns.ContainsKey(username) ? _columns[username] : string.Empty;
        }

        public override void SaveColumns(string username, string gridname, string newgridname, string columns)
        {
            _columns[username] = columns;
        }

        public override void DeleteColumns(string username, string gridname)
        {
            
        }

        public override List<View> GetViews(string username)
        {
            return null;
        }
    }
}
