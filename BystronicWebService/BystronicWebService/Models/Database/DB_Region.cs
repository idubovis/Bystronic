using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_Region
    {
        public DB_Region()
        {
            SalesPerson = new HashSet<DB_SalesPerson>();
        }

        public int RegionId { get; set; }
        public string Region1 { get; set; }

        public ICollection<DB_SalesPerson> SalesPerson { get; set; }
    }
}
