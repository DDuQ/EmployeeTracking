using System;

namespace EmployeeTracking.Common.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public bool IsConsolidated { get; set; }
    }
}
