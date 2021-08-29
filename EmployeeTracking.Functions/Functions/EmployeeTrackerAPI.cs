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

        [FunctionName(nameof(UpdateEntry))]
        public static async Task<IActionResult> UpdateEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "employee/{id}")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for Entry: {id} received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Employee employee = JsonConvert.DeserializeObject<Employee>(requestBody);

            //Validate Employee
            TableOperation findOperation = TableOperation.Retrieve<EmployeeEntity>("TIME", id);
            TableResult findResult = await employeeTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Employee not found."
                });
            }

            //Update Employee
            EmployeeEntity employeeEntity = (EmployeeEntity)findResult.Result;
            if (!string.IsNullOrEmpty(employee.Date.ToString()))
            {
                employeeEntity.Date = employee.Date;
            }

            TableOperation updateOperation = TableOperation.Replace(employeeEntity);
            await employeeTable.ExecuteAsync(updateOperation);

            string message = $"Employee: {id}, updated in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                result = employeeEntity
            });
        }

        [FunctionName(nameof(GetAllEmployeeEntries))]
        public static async Task<IActionResult> GetAllEmployeeEntries(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employee")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            ILogger log)
        {
            log.LogInformation("Get all Employee entries received.");

            TableQuery<EmployeeEntity> query = new TableQuery<EmployeeEntity>();
            TableQuerySegment<EmployeeEntity> employees = await employeeTable.ExecuteQuerySegmentedAsync<EmployeeEntity>(query, null);

            string message = "Retrieved all Employee entries.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                result = employees
            });
        }

        [FunctionName(nameof(DeleteEntry))]
        public static async Task<IActionResult> DeleteEntry(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "employee/{id}")] HttpRequest req,
            [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] EmployeeEntity employeeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete for Entry: {id} received.");

            if (employeeEntity == null)
            {
                new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Employee not found."
                });
            }

            //Delete Employee
            await employeeTable.ExecuteAsync(TableOperation.Delete(employeeEntity));

            string message = $"Employee: {employeeEntity.RowKey}, has been deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                result = employeeEntity
            });
        }

        [FunctionName(nameof(GetEntryById))]
        public static Task<IActionResult> GetEntryById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "employee/{id}")] HttpRequest req,
            [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] EmployeeEntity employeeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get Entry: {id} received.");

            if (employeeEntity == null)
            {
                new BadRequestObjectResult(new Response
                {
                    isSuccess = false,
                    message = "Employee not found."
                });
            }

            string message = $"Employee: {employeeEntity.RowKey}, has been retrieved.";
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
