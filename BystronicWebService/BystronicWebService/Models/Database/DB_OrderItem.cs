using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_OrderItem
    {
        public int OrderItemId { get; set; }
        public int? OrderId { get; set; }
        public DateTime? DateAdded { get; set; }
        public int ProductId { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? ToolingPrice { get; set; }

        public DB_Order Order { get; set; }
        public DB_Product Product { get; set; }
    }
}
