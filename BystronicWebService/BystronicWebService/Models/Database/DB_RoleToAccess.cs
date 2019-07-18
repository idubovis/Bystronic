using System;
using System.Collections.Generic;

namespace BystronicWebService.Models.Database
{
    public partial class DB_RoleToAccess
    {
        public int RoleToAccessId { get; set; }
        public string Adrole { get; set; }
        public string AppAccess { get; set; }
    }
}
