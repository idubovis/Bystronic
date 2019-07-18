using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BystronicDataService
{
    public class BystronicData
    {
        private static object _bystronicDataLock = new object();
        private static BystronicData _bystronicData;
        private static BystronicData _tmpBystronicData;

        public static BystronicData GetData(DataSource dataSource)
        {
            lock(_bystronicDataLock)
            {
                if(_bystronicData == null) 
                    _bystronicData = new BystronicData(dataSource);
                return _bystronicData;
            }
        }

        public static void UpdateData(DataSource dataSource)
        {
            //if (_tmpBystronicData == null)
                _tmpBystronicData = new BystronicData(dataSource);
           // else
            //    _tmpBystronicData.ReadData(dataSource);
            lock (_bystronicDataLock)
            {
                _bystronicData = _tmpBystronicData;
                //LogUtil.Trace("* Bystronic data reloaded");
            }
        }

        public List<User> salesmen;
        public List<Customer> customers;
        public List<Product> products;
        public List<OrderTemplate> orderTemplates;
        public List<ProductType> productTypes;
        public List<Order> orders;

        public string serializedData;

        private DataSource _dataSource;

        public BystronicData(DataSource dataSource)
        {
            _dataSource = dataSource;
            ReadData(dataSource);
        }

        public void ReadData(DataSource dataSource)
        {
            try
            {
                ReadInfo(dataSource);
                ReadOrders(dataSource);

                serializedData = serializeResponse(null);
            }
            catch(Exception e)
            {
                LogUtil.Trace($"Service could not load the database.\n{e.StackTrace}" );
            }
        }

        public void ReadInfo(DataSource dataSource)
        {
            salesmen = dataSource.GetSalesmenFor(null);
            customers = dataSource.GetCustomers();
            products = dataSource.GetProducts();
            orderTemplates = dataSource.GetOrderTemplates();
            productTypes = dataSource.GetProductTypes();
        }

        public void ReadOrders(DataSource dataSource)
        {
            orders = dataSource.GetAllOrders();
            foreach (var order in orders)
            {
                order.CheckIsCommissionsOverriden();
                order.YtdSaleBeforeThisOrder = dataSource.GetYtdSale(order.Salesman, order.ID, order.OrderDate.Value);
            }
            salesmen.ForEach(x => x.UpdateSales(orders));
        }

        public void RefreshSalesmenData(DataSource dataSource)
        {
            salesmen = dataSource.GetSalesmenFor(null);
            salesmen.ForEach(x => x.UpdateSales(orders));
        }

        public void AddOrder(Order order)
        {
            lock (_bystronicDataLock)
            {
                order.CheckIsCommissionsOverriden();
                orders.Add(order);
            }
            new Thread(() => serializedData = serializeResponse(null)).Start();
        }

        public void UpdateOrder(Order order, int index)
        {
            lock (_bystronicDataLock)
            {
                order.CheckIsCommissionsOverriden();
                orders[index] = order;
            }
            new Thread(() => serializedData = serializeResponse(null)).Start();
        }

        public void DeleteOrder(int orderID, out string poNumber)
        {
            lock (_bystronicDataLock)
            {
                var order = orders.Find(o => o.ID == orderID);
                poNumber = order.PONumber;
                orders.Remove(order);
            }
            new Thread(() => serializedData = serializeResponse(null)).Start();
        }

        public void UpdateInfo()
        {
            lock (_bystronicDataLock)
            {
                ReadInfo(BystronicDataService.DataSource);
            }
            new Thread(() => serializedData = serializeResponse(null)).Start();
        }

        public Dictionary<string,object> ToDictionary(User user)
        {
            if (user != null)
            {
                orders = _dataSource.GetOrdersFor(orders, user);
                salesmen = _dataSource.GetSalesmenFor(salesmen, user);
            }
            return new Dictionary<string, object> { { "Salesmen", salesmen }, { "Customers", customers }, { "Products", products }, { "Templates", orderTemplates }, { "ProductTypes", productTypes }, { "Orders", orders } };
        }

        public string serializeResponse(User user)
        {
            lock (_bystronicDataLock)
            {
                var responseString = BystronicDataService.serialize(new ResponseMessage { Status = true, Query = "getdata", Data = ToDictionary(user) });
                return responseString;
            }
        }

        public ResponseMessage serializeResponseMessage(User user)
        {
            return new ResponseMessage { Status = true, Query = "getdata", Data = ToDictionary(user) };
        }
    }

    public class SalesGoal
    {
        public int Year { get; set; }
        public double Target { get; set; }

        public SalesGoal(int year, double target)
        {
            Year = year;
            Target = target;
        }

        public SalesGoal(string salesGoalString)
        {
            var values = salesGoalString.Split(',');
            Year = Convert.ToInt32(values[0]);
            Target = Convert.ToDouble(values[1]);
        }

        public SalesGoal(Dictionary<string, object> data)
        {
            Year = Convert.ToInt32(data["Year"]);
            Target = Convert.ToDouble(data["Target"]);
        }

        public bool isValid()
        {
            return Target != default(double);
        }

        public override string ToString()
        {
            return Year.ToString() + "," + Target.ToString();
        }
    };

    public class User
    {
        public const string ROLE_DEALER = "Dealer";
        public const string ROLE_DSE = "DSE";
        public const string ROLE_REGIONAL_MANAGER = "RSM";
        public const string ROLE_REGIONAL_MANAGER_DSE = "RSMDSE";
        public const string ROLE_PRODUCT_MANAGER = "PM";
        public const string ROLE_APPROVER = "Approver";
        public const string ROLE_ADMINISTRATOR = "Administrator";

        public const int StartYear = 2018;

        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Role { get; private set; }
        public string Region { get; private set; }
        public string CostCenter { get; private set; }
        public double YtdSale { get; private set; }

        public List<SalesGoal> SalesGoals { get; private set; }

        public List<double> Sales { get; private set; } = new List<double>();

        public double CurrentYearSalesGoal => GetSalesGoal(DateTime.Today.Year);

        public double GetSalesGoal(int year)
        {
            var salesGoal = SalesGoals.Find(x => x.Year == year);
            return salesGoal != null ? salesGoal.Target : 0;
        }

        public void UpdateSales(List<Order> orders)
        {
            Sales.Clear();
            for (var year = StartYear; year <= DateTime.Today.Year; year++)
            {
                var salesSum = CalculateSaleSum(orders, year);
                Sales.Add(salesSum);
                if (year == DateTime.Today.Year) YtdSale = salesSum;
            }
        }

        private double CalculateSaleSum(List<Order> orders, int year)
        {
            if (Role == ROLE_REGIONAL_MANAGER)
                return orders.Where(x => x.OrderDate.Value.Year == year && x.RSM == ID).Sum(x => x.SalePrice);
            else if (Role == ROLE_REGIONAL_MANAGER_DSE)
                return orders.Where(x => x.OrderDate.Value.Year == year && (x.RSM == ID || x.Salesman == ID || x.Salesman2 == ID)).Sum(x => x.SalePrice);
            else if (Role == ROLE_DSE || Role == ROLE_DEALER)
                return orders.Where(x => x.OrderDate.Value.Year == year && (x.Salesman == ID || x.Salesman2 == ID)).Sum(x => x.SalePrice);
            return 0;
        }

        //public User(string id, string name, string role, string region, string costCenter = "", double ytdSale = 0.0, List<SalesGoal> salesGoals = null)
        //{
        //    ID = id;
        //    Name = name;
        //    Role = role;
        //    Region = region;
        //    CostCenter = costCenter;
        //    YtdSale = ytdSale;
        //    SalesGoals = salesGoals ?? new List<SalesGoal>();

        //    if(salesGoals != null)
        //    {
        //        Console.WriteLine(SalesGoalsAsString());
        //    }
        //}

        public User(string id, string name, string role, string region, string costCenter = "", double ytdSale = 0.0, string salesGoalsString = null)
        {
            ID = id;
            Name = name;
            Role = role;
            Region = region;
            CostCenter = costCenter;
            YtdSale = ytdSale;
            SalesGoals = new List<SalesGoal>();
            if (salesGoalsString != null)
            {
                var salesGoals = salesGoalsString.Split(';').ToList<string>();
                salesGoals.FindAll(sg => !string.IsNullOrEmpty(sg)).ForEach(sg =>
                {
                    SalesGoals.Add(new SalesGoal(sg));
                });
            }

            //if (SalesGoals.Count > 0)
            //{
            //    Console.WriteLine(SalesGoalsAsString());
            //}
        }

        public User(Dictionary<string, object> data)
        {
            ID = data["ID"] as string;
            Name = data["Name"] as string;
            Role = data["Role"] as string;
            Region = data["Region"] as string;
            if (data.ContainsKey("CostCenter"))
                CostCenter = data["CostCenter"] as string;
            if (data.ContainsKey("YtdSale"))
                YtdSale = Convert.ToDouble(data["YtdSale"]);
            SalesGoals = new List<SalesGoal>();
            if (data.ContainsKey("SalesGoals"))
            {
                var salesGoalsData = data["SalesGoals"] as object[];
                foreach (var goal in salesGoalsData)
                    SalesGoals.Add(new SalesGoal(goal as Dictionary<string, object>));
            }
        }

        public string SalesGoalsAsString()
        {
            return string.Join(";", SalesGoals.FindAll(sg => sg.isValid()));
        }
    }

    public class View
    {
        public string Name { get; private set; }
        public string Columns { get; private set; }

        public View (string name, string columns)
        {
            Name = name;
            Columns = columns;
        }
    }

    public class Permissions
    {
        public bool CanCreateOrder { get; private set; }
        public bool CanEditOrder { get; private set; }
        public bool CanApproveOrder { get; private set; }
        public bool CanReleaseOrder { get; private set; }
        public bool CanPayOrder { get; private set; }
        public string UserGroupString { get { return string.Join(",", _userGroups);  }  }

        private List<string> _userGroups;

        public Permissions() : this (new List<string>())
        {
        }

        public Permissions(List<string> userGroups) : this(userGroups, Config.Country_US)
        {
        }

        public Permissions(List<string> userGroups, string country)
        {
            _userGroups = userGroups;
            if (country == Config.Country_CA)
                CheckPermissionsCanada();
            else
                CheckPermissions();
        }

        private void CheckPermissions()
        {
            CanCreateOrder = _userGroups.Contains("Editors");
            CanEditOrder = _userGroups.Contains("Modifiers");
            CanApproveOrder = _userGroups.Contains("Approvers");
            CanReleaseOrder = _userGroups.Contains("Releasers");
            CanPayOrder = _userGroups.Contains("Payers");
        }

        private void CheckPermissionsCanada()
        {
            CanCreateOrder = _userGroups.Contains("EditorsCAN");
            CanEditOrder = _userGroups.Contains("ModifiersCAN");
            CanApproveOrder = _userGroups.Contains("ApproversCAN");
            CanReleaseOrder = _userGroups.Contains("ReleasersCAN");
            CanPayOrder = _userGroups.Contains("PayersCAN");
        }
    }

    public class Customer
    {
        public int ID { get; set; }
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }
        public string SapNumber { get; private set; }

        public Customer(int id, string name, string address, string city, string state, string zipCode, string sapNumber)
        {
            ID = id;
            Name = name;
            Address = address;
            City = city;
            State = state;
            ZipCode = zipCode;
            SapNumber = sapNumber;
        }

        public Customer (Dictionary<string, object> data)
        {
            if(data.ContainsKey("ID"))
                ID = (int)(data["ID"]);
            Name = data["Name"] as string;
            Address = data["Address"] as string;
            City = data["City"] as string;
            State = data["State"] as string;
            ZipCode = data["ZipCode"] as string;
            SapNumber = data["SapNumber"] as string;
        }
    }

    public class Item
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public double Value { get; set; }

        public const int TYPE_PRICE = 0;
        public const int TYPE_PERCENT = 1;
        public const int TYPE_NUMBER = 2;

        public Item(int id, int type, string name) : this(id, type, name, 0.0)
        {
        }

        public Item(int id, int type, string name, double value)
        {
            ID = id;
            Type = type;
            Name = name;
            Value = value;
        }

        public Item(Dictionary<string, object> data)
        {
            if (data.ContainsKey("ID"))
                ID = (int)(data["ID"]);
            Type = (int)data["Type"];
            Name = data["Name"] as string;
            Value = Convert.ToDouble(data["Value"]);
        }
    }

    public class Product : Item
    {
        public int ProductTypeID { get; private set; }
        public bool HasTooling { get; private set; }
        public Product(int id, int productTypeID,  string name) : this(id, productTypeID, name, 0)
        {
        }

        public Product(int id, int productTypeID, string name, double price) : this(id, productTypeID, name, price, productTypeID == 3)
        {
        }

        public Product(int id, int productTypeID, string name, double price, bool hasTooling) : base(id, Item.TYPE_PRICE, name, price)
        {
            ProductTypeID = productTypeID;
            HasTooling = hasTooling;
        }

        public Product(Dictionary<string, object> data) : base(data)
        {
            ProductTypeID = (int)data["ProductTypeID"];
            if (data.ContainsKey("HasTooling"))
                HasTooling = (bool)data["HasTooling"];
            else
                HasTooling = ProductTypeID == 3;
        }
    }

    public class ProductType
    {
        public int ID { get; private set; }
        public string Name { get; private set; }

        public ProductType(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }

    public class OutputItem : Item
    {
        public string Variable { get; private set; }
        public OutputItem(int id, int type, string name, string variable) : base(id, type, name)
        {
            Variable = variable;
        }
    }

    public class OrderItem
    {
        public int ProductID { get; private set; }
        public double Value { get; private set; }
        public double ToolingValue { get; private set; }

        public OrderItem(int productID) : this(productID, 0.0)
        {
        }

        public OrderItem(int productID, double value) : this(productID, value, 0.0)
        {
        }

        public OrderItem(int productID, double value, double toolingValue)
        {
            ProductID = productID;
            Value = value;
            ToolingValue = toolingValue;
        }

        public OrderItem(Dictionary<string, object> data)
        {
            ProductID = (int)data["ProductID"];
            Value = Convert.ToDouble(data["Value"]);
            ToolingValue = data.ContainsKey("ToolingValue") ? Convert.ToDouble(data["ToolingValue"]) : 0;
        }
    }

    public class OrderTemplate
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public bool HasTooling { get; private set; }
        public string ProductManager { get; private set; }

        public OrderTemplate(int id, string name, bool hasTooling, string productManager)
        {
            ID = id;
            Name = name;
            HasTooling = hasTooling;
            ProductManager = productManager;
        }
    }

    public class Order
    {
        public const string STATUS_PENDING = "pending";

        public const int DEALER_SALE = 0;
        public const int DIRECT_SALE_DSE = 1;
        public const int DIRECT_SALE_RSMDSE = 2;

        public int ID { get; set; }
        public int JobNumber { get; private set; }
        public int TypeOfSale { get; private set; }
        public int TemplateID { get; private set; }
        public int CustomerID { get; private set; }
        public DateTime? OrderDate { get; private set; }
        public string PONumber { get; private set; }
        public string TrackingNumber { get; private set; }
        public string Status { get; private set; }
        public string Salesman { get; private set; }
        public string Salesman2 { get; private set; }
        public string RSM { get; private set; }
        public string ApprovedBy { get; private set; }
        public string ReleasedBy { get; private set; }
        public string PaidBy { get; private set; }
        public DateTime? ApprovedDate { get; private set; }
        public DateTime? ReleasedDate { get; private set; }
        public DateTime? PayDate { get; private set; }
        public DateTime? PayrollDate { get; private set; }
        public string Comment { get; private set; }
        public List<OrderItem> OrderItems { get; private set; }
        public double TotalListPrice { get; private set; }
        public bool HasTooling { get { return GetTemplate().HasTooling; } }
        public double TotalToolingPrice { get { return HasTooling ? OrderItems.Sum(x => x.ToolingValue) : 0; } }
        public double TotalEquipmentPrice { get { return HasTooling ? TotalListPrice - TotalToolingPrice : TotalListPrice; } }
        public bool IsXpert40orXactMachine { get { return IsOrderForXpert40orXactMachine(); } }
        public double SalePrice { get; private set; }
        public double DiscountAmount { get; private set; }
        public double DiscountPercent { get; private set; }
        public double DealerCommission { get; private set; }
        public double RegionalManagerCommission { get; private set; }
        public double ProductManagerCommission { get; private set; }
        public double CalculatedDealerCommission { get; private set; }
        public double CalculatedRegionalManagerCommission { get; private set; }
        public double CalculatedProductManagerCommission { get; private set; }
        public double DSECommissionRate
        {
            get
            {
                if (SalePrice == 0) return 0;
                return Math.Round(DealerCommission / SalePrice * 100, 2, MidpointRounding.AwayFromZero);
            }
        }

        public string SalesOrder { get; private set; }
        public DateTime? EstimatedShipDate { get; private set; }
        public DateTime? FinalPaymentDate { get; private set; }

        public bool IsDealerCommissionOverriden => DealerCommission != CalculatedDealerCommission;
        public bool IsRSMCommissionOverriden => RegionalManagerCommission != CalculatedRegionalManagerCommission;
        public bool IsPMCommissionOverriden => ProductManagerCommission != CalculatedProductManagerCommission;

        public bool PayRSMCommission { get; private set; } = true;

        public double YtdSaleBeforeThisOrder { get; set; }

        [NonSerialized]
        private OrderTemplate _template;
        /*
        public string DSECommissionFormula {
            get
            {
                if (TypeOfSale == DEALER_SALE || Salesman == null || OrderDate == null) return string.Empty;
                var ytdSale = BystronicDataService.DataSource.GetYtdSale(Salesman, ID, OrderDate.Value);
                var ytdSaleWithCurrentOrder = ytdSale + SalePrice;
                string dse_commission_formula;
                if (ytdSaleWithCurrentOrder < 2000000)
                {
                    var dse_commission = ytdSaleWithCurrentOrder * 0.0025;
                    dse_commission_formula = "$" + ytdSaleWithCurrentOrder + '*' + 0.25 + "% = $" + dse_commission;
                }
                else if (ytdSaleWithCurrentOrder >= 2000000 && ytdSaleWithCurrentOrder < 4000000)
                {
                    var dse_commission = 2000000 * 0.0025 + (ytdSaleWithCurrentOrder - 2000000) * 0.0075;
                    dse_commission_formula = "$" + 2000000 + '*' + 0.25 + "% + " + (ytdSaleWithCurrentOrder-2000000) + "*" + 0.75 + "% = $" + dse_commission;
                }
                else if (ytdSaleWithCurrentOrder >= 4000000 && ytdSaleWithCurrentOrder < 6000000)
                {
                    var dse_commission = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytdSaleWithCurrentOrder - 4000000) * 0.015;
                    dse_commission_formula = "$" + 2000000 + '*' + 0.25 + "% + " + 2000000 + '*' + 0.75 + "% + " + (ytdSaleWithCurrentOrder - 4000000) + "*" + 1.5 + "% = $" + dse_commission;
                }
                else
                {
                    var dse_commission = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytdSaleWithCurrentOrder - 6000000) * 0.2;
                    dse_commission_formula = "$" + 200000 + '*' + 0.25 + "% + " + 200000 + '*' + 0.75 + "% + " + 200000 + '*' + 1.5 + "% + " + (ytdSaleWithCurrentOrder - 6000000) + "*" + 2 + "% = $" + dse_commission;
                }
                return dse_commission_formula;
            }
        }*/

        public static Order CreateOrder(int templateID)
        {
            return new Order(GenerateNewID(), GenerateNewJobNumber(), templateID, STATUS_PENDING, null);
        }

        private static int GenerateNewID()
        {
            return 0;
        }
        private static int GenerateNewJobNumber()
        {
            // needs to do it based on existing orders
            return new Random(DateTime.Now.Millisecond).Next(4000, 5000);
        }

        public Order(int id, int jobNumber, int templateID, string status, DateTime? orderDate)
            : this(id, jobNumber, templateID, status, orderDate,
                  null, null, null, true, -1, null, null, null, null, null, null, null, null, null, null,
                  new List<OrderItem>(), 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, null, null, null)
        {
        }

        public Order(int id, int jobNumber, int templateID, string status, DateTime? orderDate, string salesman, string salesman2, string rsm, bool payRSMCommission,
                     int customerID, string poNumber, string trackingNumber,
                     string approvedBy, DateTime? approvedDate, string releasedBy, DateTime? releasedDate, string paidBy, DateTime? payDate, DateTime? payrollDate, string comment,
                     List<OrderItem> orderItems,
                     double totalListPrice, double salePrice, double discountAmount, double discountPercent, double dealerCommission, double regionalManagerCommission, double productManagerCommission,
                     string salesOrder, DateTime? estimatedShipDate, DateTime? finalPaymentDate)
        {
            ID = id;
            JobNumber = jobNumber;
            Salesman = salesman;
            TypeOfSale = DEALER_SALE;
            if (Salesman != null && GetSalesmanForID(Salesman).Role == User.ROLE_DSE)
                TypeOfSale = DIRECT_SALE_DSE;
            else if (Salesman != null && GetSalesmanForID(Salesman).Role == User.ROLE_REGIONAL_MANAGER_DSE)
                TypeOfSale = DIRECT_SALE_RSMDSE;
            TemplateID = templateID;
            CustomerID = customerID;
            OrderDate = orderDate;
            PONumber = poNumber;
            TrackingNumber = trackingNumber;
            Status = status;
            Salesman2 = salesman2;
            RSM = rsm;
            PayRSMCommission = payRSMCommission;
            ApprovedBy = approvedBy;
            ApprovedDate = approvedDate;
            ReleasedBy = releasedBy;
            ReleasedDate = releasedDate;
            PaidBy = paidBy;
            PayDate = payDate;
            PayrollDate = payrollDate;
            Comment = comment;

            OrderItems = orderItems;
            TotalListPrice = totalListPrice;
            SalePrice = salePrice;
            DiscountAmount = discountAmount;
            DiscountPercent = discountPercent;
            DealerCommission = dealerCommission;
            RegionalManagerCommission = regionalManagerCommission;
            ProductManagerCommission = productManagerCommission;

            SalesOrder = salesOrder;
            EstimatedShipDate = estimatedShipDate;
            FinalPaymentDate = finalPaymentDate;

            _template = BystronicDataService.DataSource.GetOrderTemplateFor(TemplateID);
        }

        public Order(Dictionary<string, object> data)
        {
            DateTime date;
            ID = (int)data["ID"];
            JobNumber = (int)data["JobNumber"];
            Salesman = data["Salesman"] as string;
            Salesman2 = data["Salesman2"] as string;
            RSM = data["RSM"] as string;
            PayRSMCommission = (bool)data["PayRSMCommission"];
            TypeOfSale = DEALER_SALE;
            if (Salesman != null && GetSalesmanForID(Salesman).Role == User.ROLE_DSE)
                TypeOfSale = DIRECT_SALE_DSE;
            else if (Salesman != null && GetSalesmanForID(Salesman).Role == User.ROLE_REGIONAL_MANAGER_DSE)
                TypeOfSale = DIRECT_SALE_RSMDSE;

            TemplateID = (int)data["TemplateID"];
            CustomerID = (int)data["CustomerID"];
            OrderDate = DateTime.Parse(data["OrderDate"] as string);
            PONumber = data["PONumber"] as string;
            TrackingNumber = data["TrackingNumber"] as string;
            Status = data["Status"] as string;
            ApprovedBy = data["ApprovedBy"] as string;
            ReleasedBy = data["ReleasedBy"] as string;
            PaidBy = data["PaidBy"] as string;
            if (DateTime.TryParse(data["ApprovedDate"] as string, out date)) ApprovedDate = date;
            if (DateTime.TryParse(data["ReleasedDate"] as string, out date)) ReleasedDate = date;
            if (DateTime.TryParse(data["PayDate"] as string, out date)) PayDate = date;
            if (DateTime.TryParse(data["PayrollDate"] as string, out date)) PayrollDate = date;
            Comment = data["Comment"] as string;

            SalesOrder = data["SalesOrder"] as string;

            if (DateTime.TryParse(data["EstimatedShipDate"] as string, out date)) EstimatedShipDate = date;
            if (DateTime.TryParse(data["FinalPaymentDate"] as string, out date)) FinalPaymentDate = date;

            _template = BystronicDataService.DataSource.GetOrderTemplateFor(TemplateID);

            OrderItems = new List<OrderItem>();
            var orderItemsData = data["OrderItems"] as object[];
            foreach (var item in orderItemsData)
                OrderItems.Add(new OrderItem(item as Dictionary<string, object>));

            try
            {
                TotalListPrice = Convert.ToDouble(data["TotalListPrice"]);
                SalePrice = Convert.ToDouble(data["SalePrice"]);
                DiscountAmount = Convert.ToDouble(data["DiscountAmount"]);
                DiscountPercent = Convert.ToDouble(data["DiscountPercent"]);
                DealerCommission = Convert.ToDouble(data["DealerCommission"]);
                RegionalManagerCommission = Convert.ToDouble(data["RegionalManagerCommission"]);
                ProductManagerCommission = Convert.ToDouble(data["ProductManagerCommission"]);
            }
            catch (Exception) { }
        }

        public static User GetSalesmanForID(string id)
        {
            if (id == null) return null;
            return BystronicDataService.DataSource.GetAllSalesmen().Find(x => x.ID.Equals(id, StringComparison.InvariantCultureIgnoreCase));
        }

        public User GetSalesman() => GetSalesmanForID(Salesman);

        public string GetRegion() => GetSalesman().Region;

        public OrderTemplate GetTemplate() => BystronicDataService.DataSource.GetOrderTemplateFor(TemplateID);

        private bool IsOrderForXpert40orXactMachine()
        {
            var products = BystronicDataService.DataSource.GetProducts();
            foreach (var orderItem in OrderItems)
            {
                var productName = products.Find(x => x.ID == orderItem.ProductID).Name;
                if (productName != null && (productName.ToLower().Contains("xpert") || productName.ToLower().Contains("xact")))
                    return true;
            }
            return false;
        }

        public void Calculate()
        {
            TotalListPrice = OrderItems.Sum(x => x.Value + x.ToolingValue);
            CalculateCommissions();
        }

        public void CalculateCommissions()
        {
            double discountAmount, discountPercent, dealerCommission, regionalManagerCommission, productManagerCommission;
            CalculateCommissions(out discountAmount, out discountPercent, out dealerCommission, out regionalManagerCommission, out productManagerCommission);

            DiscountAmount = discountAmount;
            DiscountPercent = discountPercent;
            CalculatedDealerCommission = DealerCommission = dealerCommission;
            CalculatedRegionalManagerCommission = RegionalManagerCommission = regionalManagerCommission;
            CalculatedProductManagerCommission = ProductManagerCommission = productManagerCommission;
        }

        public void CheckIsCommissionsOverriden ()
        {
            if (TotalListPrice == 0 || Salesman == null || OrderDate == null) return;

            double discountAmount, discountPercent, dealerCommission, regionalManagerCommission, productManagerCommission;
            CalculateCommissions(out discountAmount, out discountPercent, out dealerCommission, out regionalManagerCommission, out productManagerCommission);

            CalculatedDealerCommission = dealerCommission;
            CalculatedRegionalManagerCommission = regionalManagerCommission;
            CalculatedProductManagerCommission = productManagerCommission;
        }

        private void CalculateCommissions(out double discountAmount, out double discountPercent, out double dealerCommission, out double regionalManagerCommission, out double productManagerCommission)
        {
            var formula = BystronicDataService.DataSource.GetFormula(_template.ID);
            var YtdSale = BystronicDataService.DataSource.GetYtdSale(Salesman, ID, OrderDate.Value);

            var outputData = DataSource.CalculateCommissions(formula, TypeOfSale, OrderDate.Value.ToShortDateString(), TotalListPrice, SalePrice, YtdSale, TotalEquipmentPrice, TotalToolingPrice, IsXpert40orXactMachine);

            discountAmount = Math.Round(Convert.ToDouble(outputData["discount_amount"]), 2, MidpointRounding.AwayFromZero);
            discountPercent = Math.Round(Convert.ToDouble(outputData["discount_percent"]), 2, MidpointRounding.AwayFromZero);
            dealerCommission = Math.Round(Convert.ToDouble(outputData["dealer_commission"]), 2, MidpointRounding.AwayFromZero);
            regionalManagerCommission = Math.Round(Convert.ToDouble(outputData["rsm_commission"]), 2, MidpointRounding.AwayFromZero);

            var isProductManagerCommissionPresent = GetTemplate().ProductManager != null;
            productManagerCommission = isProductManagerCommissionPresent ? Math.Round(Convert.ToDouble(outputData["pm_commission"]), 2, MidpointRounding.AwayFromZero) : 0.0;
        }

        public List<string> GetChangedFields(Order otherOrder)
        {
            var changedFields = new List<string>();

            if (PONumber != otherOrder.PONumber) changedFields.Add("PONumber");
            if (CustomerID != otherOrder.CustomerID) changedFields.Add("CustomerID");
            if (OrderDate != otherOrder.OrderDate) changedFields.Add("OrderDate");
            if (TrackingNumber != otherOrder.TrackingNumber) changedFields.Add("TrackingNumber");
            if (Status != otherOrder.Status) changedFields.Add("Status");
            if (Salesman != otherOrder.Salesman) changedFields.Add("Salesman");
            if (Salesman2 != otherOrder.Salesman2) changedFields.Add("Salesman2");
            if (RSM != otherOrder.RSM) changedFields.Add("RSM");
            if (ApprovedBy != otherOrder.ApprovedBy) changedFields.Add("ApprovedBy");
            if (ReleasedBy != otherOrder.ReleasedBy) changedFields.Add("ReleasedBy");
            if (PaidBy != otherOrder.PaidBy) changedFields.Add("PaidBy");
            if (ApprovedDate != otherOrder.ApprovedDate) changedFields.Add("ApprovedDate");
            if (ReleasedDate != otherOrder.ReleasedDate) changedFields.Add("ReleasedDate");
            if (PayDate != otherOrder.PayDate) changedFields.Add("PayDate");
            if (PayrollDate != otherOrder.PayrollDate) changedFields.Add("PayrollDate");
            if (EstimatedShipDate != otherOrder.EstimatedShipDate) changedFields.Add("EstimatedShipDate");
            if (FinalPaymentDate != otherOrder.FinalPaymentDate) changedFields.Add("FinalPaymentDate");
            if (Comment != otherOrder.Comment) changedFields.Add("Comment");
            if (TotalListPrice != otherOrder.TotalListPrice) changedFields.Add("TotalListPrice/Order Items");
            if (SalePrice != otherOrder.SalePrice) changedFields.Add("SalePrice");
            if (DealerCommission != otherOrder.DealerCommission) changedFields.Add("DealerCommission");
            if (RegionalManagerCommission != otherOrder.RegionalManagerCommission) changedFields.Add("RegionalManagerCommission");
            if (ProductManagerCommission != otherOrder.ProductManagerCommission) changedFields.Add("ProductManagerCommission");
            if (SalesOrder != otherOrder.SalesOrder) changedFields.Add("SalesOrder");
            if (PayRSMCommission != otherOrder.PayRSMCommission) changedFields.Add("PayRSMCommission");

            return changedFields;
        }
    }

    public class TestFormulaOutput
    {
        public double DiscountAmount { get; private set; }
        public double DiscountPercent { get; private set; }
        public double DealerCommission { get; private set; }
        public double RSMCommission { get; private set; }
        public double PMCommission { get; private set; }

        public string Error { get; private set; }

        public TestFormulaOutput(string formula, Dictionary<string, object> parameters)
        {
            try
            {
                var output = DataSource.CalculateCommissions(formula, parameters);
                DiscountAmount = Math.Round(Convert.ToDouble(output["discount_amount"]), 2);
                DiscountPercent = Math.Round(Convert.ToDouble(output["discount_percent"]), 2);
                DealerCommission = Math.Round(Convert.ToDouble(output["dealer_commission"]), 2);
                RSMCommission = Math.Round(Convert.ToDouble(output["rsm_commission"]), 2);
                PMCommission = Math.Round(Convert.ToDouble(output["pm_commission"]), 2); 
            }
            catch (Exception e)
            {
                Error = e.Message;
            }
        }
    }
}
