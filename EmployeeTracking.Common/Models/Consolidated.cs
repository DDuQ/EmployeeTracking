using System;

namespace EmployeeTracking.Common.Models
{
    public class Consolidated
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int MinutesWorked { get; set; }
    }
}
