using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EmployeeTracking.Functions.Entities
{
    public class Employee : TableEntity
    {
        public int employeeId { get; set; }
        public DateTime date { get; set; }
        public int type { get; set; }
        public bool isConsolidated { get; set; }
    }
}
