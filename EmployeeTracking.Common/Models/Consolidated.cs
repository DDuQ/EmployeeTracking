using System;

namespace EmployeeTracking.Common.Models
{
    public class Consolidated
    {
        public int employeeId { get; set; }
        public DateTime date { get; set; }
        public int minutesWorked { get; set; }
    }
}
