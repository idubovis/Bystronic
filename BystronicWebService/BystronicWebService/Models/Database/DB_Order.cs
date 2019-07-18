using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_Order
    {
        public DB_Order()
        {
            OrderItem = new HashSet<DB_OrderItem>();
        }

        public int OrderId { get; set; }
        public DateTime? DateEntered { get; set; }
        public DateTime? DateOfSale { get; set; }
        public int? SalesPersonId { get; set; }
        public string SalesPersonAdid { get; set; }
        public string SalesPerson2Adid { get; set; }
        public int? CustomerId { get; set; }
        public string Ponumber { get; set; }
        public decimal? TotalProductCost { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal? DiscountPerc { get; set; }
        public decimal? DealerCommission { get; set; }
        public decimal? Rmcommission { get; set; }
        public decimal? Pmcommission { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ReleasedBy { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public string TrackingNumber { get; set; }
        public int? TemplateId { get; set; }
        public string PaidBy { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime? PayrollDate { get; set; }
        public string SalesOrder { get; set; }
        public DateTime? EstimatedShipDate { get; set; }
        public DateTime? FinalPaymentDate { get; set; }
        public string Rsmadid { get; set; }
        public bool? PayRsmcommission { get; set; }

        public DB_Customer Customer { get; set; }
        public ICollection<DB_OrderItem> OrderItem { get; set; }
    }
}
