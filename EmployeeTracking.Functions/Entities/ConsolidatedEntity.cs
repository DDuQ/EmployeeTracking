using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EmployeeTracking.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public bool IsConsolidated { get; set; }
    }
}
