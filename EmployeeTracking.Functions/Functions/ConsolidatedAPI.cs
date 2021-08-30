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

            string filter = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EmployeeEntity> query = new TableQuery<EmployeeEntity>().Where(filter);
            TableQuerySegment<EmployeeEntity> notConsolidatedTimes = await employeeTable.ExecuteQuerySegmentedAsync(query, null);

            List<EmployeeEntity> array = notConsolidatedTimes.Results.OrderBy(t => t.EmployeeId).ToList();
            TimeSpan consolidatedTime;
            double consolidated = 0;
            int index = 0;
            int updated = 0;

            while (index <= array.Count)
            {
                consolidated = Math.Ceiling((double)2.5);
                if (array[index].Type == IN)
                {
                    if (array[index].EmployeeId == array[index + 1].EmployeeId
                       && array[index].Type != array[index + 1].Type)
                    {
                        consolidatedTime = array[index + 1].Date.Subtract(array[index].Date);

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
    }
}
