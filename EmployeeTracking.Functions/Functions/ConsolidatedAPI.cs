using EmployeeTracking.Common.Responses;
using EmployeeTracking.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeTracking.Functions.Functions
{
    public static class ConsolidatedAPI
    {
        private const int IN = 0;
        private const int OUT = 1;

        [FunctionName(nameof(Consolidated))]
        public static async Task<IActionResult> Consolidated(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidated")] HttpRequest req,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            ILogger log)
        {
            log.LogInformation("Time consolidation received.");

            string employeeFilter = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EmployeeEntity> query = new TableQuery<EmployeeEntity>().Where(employeeFilter);
            TableQuerySegment<EmployeeEntity> notConsolidatedTimes = await employeeTable.ExecuteQuerySegmentedAsync(query, null);

            List<EmployeeEntity> employeeArray = notConsolidatedTimes.Results.OrderBy(t => t.EmployeeId).ThenBy(t => t.Date).ToList();
            TimeSpan consolidatedTime;
            int consolidated = 0;
            int index = 0;
            int minutes = 0;
            int updated = 0;

            while (index <= employeeArray.Count)
            {
                //iterations = GetNumberOfIterations(employeeArray, index);
                if (employeeArray[index].Type == IN)
                {
                    if (employeeArray[index].EmployeeId == employeeArray[index + 1].EmployeeId
                       && employeeArray[index].Type != employeeArray[index + 1].Type)
                    {
                        consolidatedTime = employeeArray[index + 1].Date - employeeArray[index].Date;
                        minutes = (int)consolidatedTime.TotalMinutes;
                        ConsolidatedEntity consolidatedRecord = GetConsolidatedByEmployeeId(employeeArray[index].EmployeeId, consolidatedTable).Result.FirstOrDefault();

                        if (consolidatedRecord != null)
                        {
                            await UpdateConsolidatedRecord(consolidatedTable, consolidatedRecord);
                        }
                        else
                        {
                            await CreateNewConsolidatedRecord(consolidatedTable, employeeArray[index], minutes);
                        }

                    }
                }
                index++;
            }

            string message = $"Consolidation summary. Records added: {consolidated}, records updated: {updated}";

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = message,
                result = null
            });
        }

        private static async Task<IActionResult> UpdateConsolidatedRecord(CloudTable consolidatedTable, ConsolidatedEntity consolidatedRecord)
        {
            TableOperation updateOperation = TableOperation.Replace(consolidatedRecord);
            await consolidatedTable.ExecuteAsync(updateOperation);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = $"Record {consolidatedRecord.EmployeeId} has been updated.",
                result = consolidatedRecord
            });
        }

        private static async Task<IActionResult> CreateNewConsolidatedRecord(CloudTable consolidatedTable, EmployeeEntity employeeEntity, int minutes)
        {
            ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity
            {
                EmployeeId = employeeEntity.EmployeeId,
                Date = DateTime.UtcNow,
                MinutesWorked = minutes,
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
            };

            TableOperation addOperation = TableOperation.Insert(consolidatedEntity);
            await consolidatedTable.ExecuteAsync(addOperation);

            return new OkObjectResult(new Response
            {
                isSuccess = true,
                message = $"Record with id: {consolidatedEntity.EmployeeId} has been created.",
                result = consolidatedEntity
            });
        }

        private static async Task<List<ConsolidatedEntity>> GetConsolidatedByEmployeeId(int employeeId, CloudTable consolidatedTable)
        {
            string consolidatedFilter = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, employeeId);
            TableQuery<ConsolidatedEntity> query = new TableQuery<ConsolidatedEntity>().Where(consolidatedFilter);
            TableQuerySegment<ConsolidatedEntity> consolidatedMatch = await consolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            return consolidatedMatch.Results;
        }
    }
}
