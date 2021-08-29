using EmployeeTracking.Common.Models;
using EmployeeTracking.Common.Responses;
using EmployeeTracking.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EmployeeTracking.Functions.Functions
{
    public static class EmployeeTrackerAPI
    {
        [FunctionName(nameof(CreateEntry))]
        public static async Task<IActionResult> CreateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "employee")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            ILogger log)
        {
            log.LogInformation("Received a new Entry");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Employee employee = JsonConvert.DeserializeObject<Employee>(requestBody);

            if (string.IsNullOrEmpty(employee?.EmployeeId.ToString()) ||
                string.IsNullOrEmpty(employee?.Date.ToString()) ||
                string.IsNullOrEmpty(employee?.Type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "The request must have the following fields: EmployeeId, Date and Type."
                });
            }

            EmployeeEntity employeeEntity = new EmployeeEntity
            {
                Date = employee.Date,
                Type = employee.Type,
                EmployeeId = employee.EmployeeId,
                ETag = "*",
                IsConsolidated = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),

            };

            TableOperation addOperation = TableOperation.Insert(employeeEntity);
            await employeeTable.ExecuteAsync(addOperation);

            string message = "New employee track stored in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                result = employeeEntity
            });
        }
    }
}
