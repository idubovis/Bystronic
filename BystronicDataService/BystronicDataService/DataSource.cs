using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BystronicDataService
{
    abstract public class DataSource
    {
        abstract public List<User> GetAllSalesmen();
        abstract public List<Customer> GetCustomers();
        abstract public List<ProductType> GetProductTypes();
        abstract public List<Product> GetProducts();
        abstract public List<Order> GetAllOrders();
        abstract public List<OrderTemplate> GetOrderTemplates();

        abstract public bool AddOrder(Order order);
        abstract public bool UpdateOrder(Order order);
        abstract public bool DeleteOrder(int orderID);

        abstract public bool AddSalesman(User salesman);
        abstract public bool UpdateSalesman(string id, User salesman);
        abstract public bool DeleteSalesman(User salesman);

        abstract public bool AddCustomer(Customer customer);
        abstract public bool UpdateCustomer(Customer customer);
        abstract public bool DeleteCustomer(Customer customer);

        abstract public bool AddProduct(Product product);
        abstract public bool UpdateProduct(Product product);
        abstract public bool DeleteProduct(Product product);

        abstract public string GetFormula(int templateID);
        abstract public bool SaveFormula(int templateID, string formula);

        abstract public double GetYtdSale(string salesmanID, int orderID, DateTime orderDate);

        abstract public List<View> GetViews(string username);
        abstract public string GetColumns(string username, string gridname = "Default");
        abstract public void SaveColumns(string username, string gridname, string newgridname, string columns);
        abstract public void DeleteColumns(string username, string gridname);

        abstract public bool IsReady();

        public List<User> GetSalesmenFor(User salesman)
        {
            return GetSalesmenFor(GetAllSalesmen(), salesman);
        }

        public List<User> GetSalesmenFor(List<User> allSalesmen, User salesman)
        {
            //if (salesman.Role == User.ROLE_DEALER || salesman.Role == User.ROLE_DSE)
            //    return new List<User>() { salesman };
            var salesmen = allSalesmen.FindAll(x => x.Role == User.ROLE_DEALER || x.Role == User.ROLE_DSE || x.Role == User.ROLE_REGIONAL_MANAGER_DSE || x.Role == User.ROLE_REGIONAL_MANAGER);
            //if (salesman.Role == User.ROLE_REGIONAL_MANAGER || salesman.Role == User.ROLE_REGIONAL_MANAGER_DSE)
            //    return salesmen.FindAll(x => x.Region == salesman.Region);
            return salesmen;
        }

        public List<Order> GetOrdersFor(List<Order> orders, User user)
        {
            var ordersForUser = orders;
            if (user.Role == User.ROLE_DEALER || user.Role == User.ROLE_DSE)
                ordersForUser = orders.FindAll(x => x.Salesman == user.ID || x.Salesman2 == user.ID);
            else if (user.Role == User.ROLE_REGIONAL_MANAGER || user.Role == User.ROLE_REGIONAL_MANAGER_DSE)
                ordersForUser = orders.FindAll(x => x.GetRegion() == user.Region);
            else if (user.Role == User.ROLE_PRODUCT_MANAGER)
                ordersForUser = orders.FindAll(x => GetOrderTemplateFor(x.TemplateID).ProductManager == user.ID);
            return ordersForUser;
        }

        public void OrderChanged(Order order)
        {
            order.YtdSaleBeforeThisOrder = this.GetYtdSale(order.Salesman, order.ID, order.OrderDate.Value);
            var index = BystronicData.GetData(this).orders.FindIndex(x => x.ID == order.ID);
            if (index >= 0)
            {
                UpdateOrder(order);
                BystronicData.GetData(this).UpdateOrder(order, index);
            }
            else
            {
                AddOrder(order);
                BystronicData.GetData(this).AddOrder(order);
            }
            BystronicData.GetData(this).RefreshSalesmenData(this);
        }

        public void OrderDeleted(int orderID, out string poNumber)
        {
            DeleteOrder(orderID);
            BystronicData.GetData(this).DeleteOrder(orderID, out poNumber);
            BystronicData.GetData(this).RefreshSalesmenData(this);
        }

        public OrderTemplate GetOrderTemplateFor(int templateID)
        {
            return GetOrderTemplates().Find(x => x.ID == templateID);
        }

        public static List<OutputItem> GetOutputItems()
        {
            return new List<OutputItem>()
            {
                new OutputItem(1001, Item.TYPE_PRICE, "Total List Price of Equipment", "list_price"),
                new OutputItem(1002, Item.TYPE_PRICE, "Selling Price According to Order Confirmation", "sale_price"),
                new OutputItem(1003, Item.TYPE_PRICE, "Discount Amount", "discount_amount"),
                new OutputItem(1004, Item.TYPE_PERCENT, "Discount %", "discount_percent"),
                new OutputItem(1005, Item.TYPE_PRICE, "Dealer's Commission", "dealer_commission"),
                new OutputItem(1006, Item.TYPE_PRICE, "Regional Manager Commission", "regional_manager_commission"),
                new OutputItem(1007, Item.TYPE_PRICE, "Product Manager Commission", "product_manager_commission")
            };
        }

        public static List<OutputItem> GetOutputItems(string role)
        {
            var outputItems = new List<OutputItem>()
            {
                new OutputItem(1001, Item.TYPE_PRICE, "Total List Price of Equipment", "list_price"),
                new OutputItem(1002, Item.TYPE_PRICE, "Selling Price According to Order Confirmation", "sale_price"),
                new OutputItem(1003, Item.TYPE_PRICE, "Discount Amount", "discount_amount"),
                new OutputItem(1004, Item.TYPE_PERCENT, "Discount %", "discount_percent")
            };

            if (role == User.ROLE_DEALER || role == User.ROLE_DSE || role == User.ROLE_APPROVER || role == User.ROLE_ADMINISTRATOR)
                outputItems.Add(new OutputItem(1005, Item.TYPE_PRICE, "Dealer's Commission", "dealer_commission"));
            if (role == User.ROLE_REGIONAL_MANAGER || role == User.ROLE_APPROVER || role == User.ROLE_ADMINISTRATOR)
                outputItems.Add(new OutputItem(1006, Item.TYPE_PRICE, "Regional Manager Commission", "regional_manager_commission"));
            if (role == User.ROLE_PRODUCT_MANAGER || role == User.ROLE_APPROVER || role == User.ROLE_ADMINISTRATOR)
                outputItems.Add(new OutputItem(1007, Item.TYPE_PRICE, "Product Manager Commission", "product_manager_commission"));

            return outputItems;
        }

        public static Dictionary<string, double> CalculateCommissions(string formula, Dictionary<string, object> parameters)
        {
            return CalculateCommissions(formula, Convert.ToInt32(parameters["TypeOfSale"]),
                    parameters["OrderDate"] as string,
                    Convert.ToDouble(parameters["ListPrice"]), Convert.ToDouble(parameters["SalePrice"]), Convert.ToDouble(parameters["YtdSale"]),
                    Convert.ToDouble(parameters["EquipmentPrice"]), Convert.ToDouble(parameters["ToolingPrice"]), Convert.ToBoolean(parameters["IsXpert40orXactMachine"]));
        }

        public static Dictionary<string, double> CalculateCommissions(string formula, int typeOfSale, string orderDate, double totalListPrice, double salePrice, double ytdSale, double totalEquipmentPrice, double totalToolingPrice, bool isXpert40orXactMachine)
        {
            var calculationEngine = new JintEngine();
            
            var functionCall = GetFunctionCall(typeOfSale, orderDate, totalListPrice, salePrice, ytdSale, totalEquipmentPrice, totalToolingPrice, isXpert40orXactMachine);
            Console.WriteLine(functionCall);

            calculationEngine.SetDebugMode(true).Run(formula);
            return (calculationEngine.Run(functionCall) as JsObject).ToDictionary(x => x.Key, x => x.Value.ToNumber());
        }

        public static string GetFunctionCall(int typeOfSale, string orderDate, double totalListPrice, double salePrice, double ytdSale, double totalEquipmentPrice, double totalToolingPrice, bool isXpert40orXactMachine)
        {
            return "calculate_commission(" + typeOfSale + ",'" + orderDate + "'," + totalListPrice + "," + salePrice + "," + ytdSale + "," + totalEquipmentPrice + "," + totalToolingPrice + "," + isXpert40orXactMachine.ToString().ToLower() + ");";
        }

        public string GetDefaultFormula(int templateID)
        {
            switch (templateID)
            {
                case 1: // Laser
                    return
  @"function calculate_commission(type_of_sale, order_date, list_price, sale_price, ytd_sale) 
{ 
    orderdate = new Date(order_date);  

    discount_amount = list_price - sale_price;
    if (discount_amount < 0) 
        discount_amount = 0;
    discount = discount_amount / list_price;

    if(type_of_sale == 0) 
    {   // dealer sale 
        if (discount > 0.08)
        {
            amount_over_8_percent = discount_amount - (list_price * 0.08);
            dealer_discount_share = amount_over_8_percent / 2;
            sale_at_8_percent_discount = list_price - (list_price * 0.08);
            temp_dealer_commission = sale_at_8_percent_discount * 0.08;
            dealer_commission = temp_dealer_commission - dealer_discount_share;
        }
        else 
            dealer_commission = sale_price * 0.08;

        minimum_dealer_commission = 20000;
        if(orderdate >= new Date('02/18/2018')) {
            // apply new minimum dealer commission schedule (as of Feb 18, 2018)   
            minimum_dealer_commission = 5000;
            if(sale_price > 200000)
                minimum_dealer_commission = 10000;
            if(sale_price > 400000)
                minimum_dealer_commission = 20000;
            if(sale_price > 1000000)
                minimum_dealer_commission = 30000;
        }
        if (dealer_commission < minimum_dealer_commission) 
            dealer_commission = minimum_dealer_commission;

    }
    else 
    {   // direct sale
        // sliding commission calculation
        if (ytd_sale < 2000000) {
            dealer_commission_ytd = ytd_sale * 0.0025;
        }
        else if (ytd_sale >= 2000000 && ytd_sale < 4000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + (ytd_sale - 2000000) * 0.0075;
        }
        else if (ytd_sale >= 4000000 && ytd_sale < 6000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale - 4000000) * 0.015;
        }
        else  {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale - 6000000) * 0.020;
        }
            
        ytd_sale_with_current_order = ytd_sale + sale_price; 
        
        if (ytd_sale_with_current_order < 2000000) {
            dealer_commission_ytd_with_current_order =  ytd_sale_with_current_order * 0.0025;
        }
        else if ( ytd_sale_with_current_order >= 2000000 &&  ytd_sale_with_current_order < 4000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + ( ytd_sale_with_current_order - 2000000) * 0.0075;
        }
        else if ( ytd_sale_with_current_order >= 4000000 &&  ytd_sale_with_current_order < 6000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + ( ytd_sale_with_current_order - 4000000) * 0.015;
        }
        else {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + ( ytd_sale_with_current_order - 6000000) * 0.020;  
        }
        dealer_commission = dealer_commission_ytd_with_current_order - dealer_commission_ytd;    
    }

    if(type_of_sale == 2)
        rsm_commission = sale_price * 0.01;
    else
        rsm_commission = sale_price * 0.004;
    
    if(orderdate >= new Date('02/04/2019')) 
        pm_commission = sale_price * 0.00035;
    else 
        pm_commission = sale_price * 0.001;

    discount_percent = discount * 100;

    return { 
             'discount_amount' : discount_amount, 
             'discount_percent' : discount_percent, 
             'dealer_commission' : dealer_commission, 
             'rsm_commission' : rsm_commission, 
             'pm_commission' : pm_commission
    };
}
                    ";
                case 2: // Automation
                    return
  @"function calculate_commission(type_of_sale, order_date, list_price, sale_price, ytd_sale) 
{ 
    orderdate = new Date(order_date); 

    discount_amount = list_price - sale_price;
    if (discount_amount < 0) 
        discount_amount = 0;
    discount = discount_amount / list_price;

    if(type_of_sale == 0) 
    {   // dealer sale 
        if (discount > 0.08)
        {
            amount_over_8_percent = discount_amount - (list_price * 0.08);
            dealer_discount_share = amount_over_8_percent / 2;
            sale_at_8_percent_discount = list_price - (list_price * 0.08);
            temp_dealer_commission = sale_at_8_percent_discount * 0.08;
            dealer_commission = temp_dealer_commission - dealer_discount_share;
        }
        else 
            dealer_commission = sale_price * 0.08;
            
        minimum_dealer_commission = 5000;
        if(orderdate >= new Date('02/18/2018')) {
            // apply new minimum dealer commission schedule (as of Feb 18, 2018)   
            minimum_dealer_commission = 5000;
            if(sale_price > 200000)
                minimum_dealer_commission = 10000;
            if(sale_price > 400000)
                minimum_dealer_commission = 20000;
            if(sale_price > 1000000)
                minimum_dealer_commission = 30000;
        }
        if (dealer_commission < minimum_dealer_commission) 
            dealer_commission = minimum_dealer_commission;

        if (dealer_commission < minimum_dealer_commission) 
            dealer_commission = minimum_dealer_commission;
    }
    else 
    {   // direct sale
        // sliding commission calculation
        if (ytd_sale < 2000000) {
            dealer_commission_ytd = ytd_sale * 0.0025;
        }
        else if (ytd_sale >= 2000000 && ytd_sale < 4000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + (ytd_sale - 2000000) * 0.0075;
        }
        else if (ytd_sale >= 4000000 && ytd_sale < 6000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale - 4000000) * 0.015;
        }
        else {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale - 6000000) * 0.020;
        }
            
        ytd_sale_with_current_order = ytd_sale + sale_price; 
        
        if (ytd_sale_with_current_order < 2000000) {
            dealer_commission_ytd_with_current_order =  ytd_sale_with_current_order * 0.0025;
        }
        else if ( ytd_sale_with_current_order >= 2000000 &&  ytd_sale_with_current_order < 4000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + ( ytd_sale_with_current_order - 2000000) * 0.0075;
        }
        else if ( ytd_sale_with_current_order >= 4000000 &&  ytd_sale_with_current_order < 6000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale_with_current_order - 4000000) * 0.015;
        }
        else {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale_with_current_order - 6000000) * 0.020;  
        }
        dealer_commission = dealer_commission_ytd_with_current_order - dealer_commission_ytd;    
    }

    if(type_of_sale == 2)
        rsm_commission = sale_price * 0.01;
    else
        rsm_commission = sale_price * 0.004;
    if(orderdate >= new Date('02/04/2019')) 
        pm_commission = sale_price * 0.0007;
    else 
        pm_commission = sale_price * 0.001;
    discount_percent = discount * 100;

    return { 
             'discount_amount' : discount_amount, 
             'discount_percent' : discount_percent, 
             'dealer_commission' : dealer_commission, 
             'rsm_commission' : rsm_commission, 
             'pm_commission' : pm_commission
    };
}
                    ";
                case 3: // Pressbrake
                    return
  @"function calculate_commission(type_of_sale, order_date, list_price, sale_price, ytd_sale, equipment_price, tooling_price, is_xpert40_or_xact_machine) 
{
    orderdate = new Date(order_date); 

    discount_amount = list_price - sale_price;
    if (discount_amount < 0) discount_amount = 0;
    discount = discount_amount / list_price;

    machine_discount_amount = equipment_price - (sale_price - tooling_price);
    machine_discount = machine_discount_amount / equipment_price;

    if(type_of_sale == 0) 
    {   // dealer sale 
        if (machine_discount > 0.08)
        {
            amount_over_8_percent = (equipment_price - (equipment_price * 0.08)) - (sale_price - tooling_price);
            dealer_discount_share = amount_over_8_percent / 2;
            sale_at_8_percent_discount = equipment_price - (equipment_price * 0.08);
            temp_dealer_commission = sale_at_8_percent_discount * 0.09;
            dealer_machine_commission = temp_dealer_commission - dealer_discount_share;
        }
        else
            dealer_machine_commission = sale_price * 0.09;

        if(is_xpert40_or_xact_machine == true)
        {
            if (dealer_machine_commission < 5000.00)
                dealer_machine_commission = 5000.00;
        }    
        else
        {
            if (dealer_machine_commission < 10000.00)
                dealer_machine_commission = 10000.00;
        } 

        minimum_dealer_commission = 5000;
        if(orderdate >= new Date('02/18/2018')) {
            // apply new minimum dealer commission schedule (as of Feb 18, 2018)   
            minimum_dealer_commission = 5000;
            if(sale_price > 200000)
                minimum_dealer_commission = 10000;
            if(sale_price > 400000)
                minimum_dealer_commission = 20000;
            if(sale_price > 1000000)
                minimum_dealer_commission = 30000;
        }
        
        if (dealer_machine_commission < minimum_dealer_commission) 
            dealer_machine_commission = minimum_dealer_commission;
            
        if(tooling_price > 0) // order contains tooling
        {
            dealer_tooling_commission = (tooling_price * 0.2) - (tooling_price * machine_discount) / 2;
            dealer_commission = dealer_machine_commission + dealer_tooling_commission;
        }
        else
            dealer_commission = dealer_machine_commission;

        rsm_commission = sale_price * 0.004;
       //pm_commission = sale_price * 0.001;
        pm_commission = equipment_price * (1 - discount) * 0.001 + tooling_price * (1 - discount) * 0.01;
    }
    else 
    {   // direct sale
        // sliding commission calculation
        if(type_of_sale == 2)
            rsm_commission = sale_price * 0.01;
        else
            rsm_commission = sale_price * 0.004;
        if(tooling_price > 0) // order contains tooling
        {
            machine_sale_price = sale_price - tooling_price;

            if (ytd_sale < 2000000) 
                dealer_machine_commission_ytd = ytd_sale * 0.0025;
            else if (ytd_sale >= 2000000 && ytd_sale < 4000000) 
                dealer_machine_commission_ytd = 2000000 * 0.0025 + (ytd_sale - 2000000) * 0.0075;
            else if (ytd_sale >= 4000000 && ytd_sale < 6000000) 
                dealer_machine_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale - 4000000) * 0.015;
            else 
                dealer_machine_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale - 6000000) * 0.020;
            
            ytd_sale_with_current_order = ytd_sale + machine_sale_price; 
        
            if (ytd_sale_with_current_order < 2000000) {
                dealer_machine_commission_ytd_with_current_order =  ytd_sale_with_current_order * 0.0025;
            }
            else if ( ytd_sale_with_current_order >= 2000000 &&  ytd_sale_with_current_order < 4000000) {
                dealer_machine_commission_ytd_with_current_order = 2000000 * 0.0025 + ( ytd_sale_with_current_order - 2000000) * 0.0075;
            }
            else if ( ytd_sale_with_current_order >= 4000000 &&  ytd_sale_with_current_order < 6000000) {
                dealer_machine_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale_with_current_order - 4000000) * 0.015;
            }
            else {
                dealer_machine_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale_with_current_order - 6000000) * 0.020;  
            }

            dealer_machine_commission = dealer_machine_commission_ytd_with_current_order - dealer_machine_commission_ytd;

            dealer_commission = dealer_machine_commission + (tooling_price * (1 - discount) * 0.05);

            pm_commission = tooling_price * (1 - discount) * 0.01 + (equipment_price - tooling_price) * (1 - discount) * 0.001;
        }
        else
        {               
            if (ytd_sale < 2000000) 
                dealer_commission_ytd = ytd_sale * 0.0025;
            else if (ytd_sale >= 2000000 && ytd_sale < 4000000) 
                dealer_commission_ytd = 2000000 * 0.0025 + (ytd_sale - 2000000) * 0.0075;
            else if (ytd_sale >= 4000000 && ytd_sale < 6000000) 
                dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale - 4000000) * 0.015;
            else 
                dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale - 6000000) * 0.020;
            
            ytd_sale_with_current_order = ytd_sale + sale_price; 
        
            if (ytd_sale_with_current_order < 2000000) {
                dealer_commission_ytd_with_current_order =  ytd_sale_with_current_order * 0.0025;
            }
            else if ( ytd_sale_with_current_order >= 2000000 &&  ytd_sale_with_current_order < 4000000) {
                dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + ( ytd_sale_with_current_order - 2000000) * 0.0075;
            }
            else if ( ytd_sale_with_current_order >= 4000000 &&  ytd_sale_with_current_order < 6000000) {
                dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale_with_current_order - 4000000) * 0.015;
            }
            else {
                dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale_with_current_order - 6000000) * 0.020;  
            }
            dealer_commission = dealer_commission_ytd_with_current_order - dealer_commission_ytd;    

            pm_commission = sale_price * 0.001;
        }
    }
    discount_percent = discount * 100;

    return { 
             'discount_amount' : discount_amount, 
             'discount_percent' : discount_percent, 
             'dealer_commission' : dealer_commission, 
             'rsm_commission' : rsm_commission, 
             'pm_commission' : pm_commission
    };
}
                    ";
            case 6: // Tube Laser
                    return
  @"function calculate_commission(type_of_sale, order_date, list_price, sale_price, ytd_sale) 
{ 
    orderdate = new Date(order_date);  

    discount_amount = list_price - sale_price;
    if (discount_amount < 0) 
        discount_amount = 0;
    discount = discount_amount / list_price;

    if(type_of_sale == 0) 
    {   // dealer sale 
        if (discount > 0.08)
        {
            amount_over_8_percent = discount_amount - (list_price * 0.08);
            dealer_discount_share = amount_over_8_percent / 2;
            sale_at_8_percent_discount = list_price - (list_price * 0.08);
            temp_dealer_commission = sale_at_8_percent_discount * 0.08;
            dealer_commission = temp_dealer_commission - dealer_discount_share;
        }
        else 
            dealer_commission = sale_price * 0.08;

        minimum_dealer_commission = 20000;
        if(orderdate >= new Date('02/18/2018')) {
            // apply new minimum dealer commission schedule (as of Feb 18, 2018)   
            minimum_dealer_commission = 5000;
            if(sale_price > 200000)
                minimum_dealer_commission = 10000;
            if(sale_price > 400000)
                minimum_dealer_commission = 20000;
            if(sale_price > 1000000)
                minimum_dealer_commission = 30000;
        }
        if (dealer_commission < minimum_dealer_commission) 
            dealer_commission = minimum_dealer_commission;

    }
    else 
    {   // direct sale
        // sliding commission calculation
        if (ytd_sale < 2000000) {
            dealer_commission_ytd = ytd_sale * 0.0025;
        }
        else if (ytd_sale >= 2000000 && ytd_sale < 4000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + (ytd_sale - 2000000) * 0.0075;
        }
        else if (ytd_sale >= 4000000 && ytd_sale < 6000000) {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + (ytd_sale - 4000000) * 0.015;
        }
        else  {
            dealer_commission_ytd = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + (ytd_sale - 6000000) * 0.020;
        }
            
        ytd_sale_with_current_order = ytd_sale + sale_price; 
        
        if (ytd_sale_with_current_order < 2000000) {
            dealer_commission_ytd_with_current_order =  ytd_sale_with_current_order * 0.0025;
        }
        else if ( ytd_sale_with_current_order >= 2000000 &&  ytd_sale_with_current_order < 4000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + ( ytd_sale_with_current_order - 2000000) * 0.0075;
        }
        else if ( ytd_sale_with_current_order >= 4000000 &&  ytd_sale_with_current_order < 6000000) {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + ( ytd_sale_with_current_order - 4000000) * 0.015;
        }
        else {
            dealer_commission_ytd_with_current_order = 2000000 * 0.0025 + 2000000 * 0.0075 + 2000000 * 0.015 + ( ytd_sale_with_current_order - 6000000) * 0.020;  
        }
        dealer_commission = dealer_commission_ytd_with_current_order - dealer_commission_ytd;    
    }

    if(type_of_sale == 2)
        rsm_commission = sale_price * 0.01;
    else
        rsm_commission = sale_price * 0.004;
    
    if(orderdate >= new Date('02/04/2019')) 
        pm_commission = sale_price * 0.0007;
    else 
        pm_commission = sale_price * 0.001;

    discount_percent = discount * 100;

    return { 
             'discount_amount' : discount_amount, 
             'discount_percent' : discount_percent, 
             'dealer_commission' : dealer_commission, 
             'rsm_commission' : rsm_commission, 
             'pm_commission' : pm_commission
    };
}
                    ";
                default:
                    return "";
            }
        }
    }
}

