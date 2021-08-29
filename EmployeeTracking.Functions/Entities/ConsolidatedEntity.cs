using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace EmployeeTracking.Functions.Entities
{
    public class ConsolidatedEntity : TableEntity
    {
        public int employeeId { get; set; }
        public DateTime date { get; set; }
        public int minutesWorked { get; set; }
    }
}
