using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_ProductType
    {
        public DB_ProductType()
        {
            Product = new HashSet<DB_Product>();
        }

        public int ProductTypeId { get; set; }
        public string Type { get; set; }
        public decimal? CommissionPerc { get; set; }

        public ICollection<DB_Product> Product { get; set; }
    }
}
