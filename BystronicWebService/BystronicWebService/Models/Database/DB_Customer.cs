using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_Customer
    {
        public DB_Customer()
        {
            Order = new HashSet<DB_Order>();
        }

        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int? RegionId { get; set; }
        public string Sapnumber { get; set; }

        public ICollection<DB_Order> Order { get; set; }
    }
}
