using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_Product
    {
        public DB_Product()
        {
            OrderItem = new HashSet<DB_OrderItem>();
        }

        public int ProductId { get; set; }
        public int? ProductTypeId { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public bool? Tooling { get; set; }
        public string ProductManager { get; set; }

        public DB_ProductType ProductType { get; set; }
        public ICollection<DB_OrderItem> OrderItem { get; set; }
    }
}
