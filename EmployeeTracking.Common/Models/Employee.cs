using System;

namespace EmployeeTracking.Common.Models
{
    public class Employee
    {
        public int employeeId { get; set; }
        public DateTime date { get; set; }
        public int type { get; set; }
        public bool isConsolidated { get; set; }
    }
}
