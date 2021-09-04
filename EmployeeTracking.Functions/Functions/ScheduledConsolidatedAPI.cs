using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeTracking.Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace EmployeeTracking.Functions.Functions
{
    public static class ScheduledConsolidatedAPI
    {
        private const int IN = 0;
        private const int OUT = 1;

        [FunctionName("ScheduledConsolidated")]
        public static async Task Run(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table("consolidated", Connection = "AzureWebJobsStorage")] CloudTable consolidatedTable,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable employeeTable,
            ILogger log)
        {
            log.LogInformation("Time scheduled consolidation received.");

            string employeeFilter = TableQuery.GenerateFilterConditionForBool("IsConsolidated", QueryComparisons.Equal, false);
            TableQuery<EmployeeEntity> query = new TableQuery<EmployeeEntity>().Where(employeeFilter);
            TableQuerySegment<EmployeeEntity> notConsolidatedTimes = await employeeTable.ExecuteQuerySegmentedAsync(query, null);

            List<EmployeeEntity> employeeArray = notConsolidatedTimes.Results.OrderBy(t => t.EmployeeId).ThenBy(t => t.Date).ToList();
            TimeSpan consolidatedTime;
            int consolidated = 0;
            int index = 0;
            int minutes = 0;
            int updated = 0;

            while (index < employeeArray.Count)
            {
                if (employeeArray[index].Type == IN && employeeArray[index] != employeeArray.Last())
                {
                    if (employeeArray[index].EmployeeId == employeeArray[index + 1].EmployeeId
                       && employeeArray[index].Type != employeeArray[index + 1].Type)
                    {
                        consolidatedTime = employeeArray[index + 1].Date - employeeArray[index].Date;
                        minutes = (int)consolidatedTime.TotalMinutes;

                        ConsolidatedEntity consolidatedRecord = GetConsolidatedByEmployeeIdAsync(employeeArray[index].EmployeeId, consolidatedTable).Result.FirstOrDefault();

                        if (consolidatedRecord != null)
                        {
                            await UpdateConsolidatedRecordAsync(consolidatedTable, consolidatedRecord, minutes);
                            await UpdateIsConsolidatedForEmployeesAsync(employeeArray[index], employeeArray[index + 1], employeeTable);
                            updated++;
                        }
                        else
                        {
                            await CreateNewConsolidatedRecordAsync(consolidatedTable, employeeArray[index], minutes);
                            await UpdateIsConsolidatedForEmployeesAsync(employeeArray[index], employeeArray[index + 1], employeeTable);
                            consolidated++;
                        }

                    }
                }
                index++;
            }

            log.LogInformation($"Consolidation summary. Records added: {consolidated}, records updated: {updated}");
        }

        private static async Task UpdateIsConsolidatedForEmployeesAsync(EmployeeEntity employeeEntity1, EmployeeEntity employeeEntity2, CloudTable employeeTable)
        {
            employeeEntity1.IsConsolidated = true;
            employeeEntity2.IsConsolidated = true;

            TableOperation updateOperation1 = TableOperation.Replace(employeeEntity1);
            TableOperation updateOperation2 = TableOperation.Replace(employeeEntity2);
            await employeeTable.ExecuteAsync(updateOperation1);
            await employeeTable.ExecuteAsync(updateOperation2);
        }

        private static async Task UpdateConsolidatedRecordAsync(CloudTable consolidatedTable, ConsolidatedEntity consolidatedRecord, int minutes)
        {
            consolidatedRecord.MinutesWorked += minutes;
            TableOperation updateOperation = TableOperation.Replace(consolidatedRecord);
            await consolidatedTable.ExecuteAsync(updateOperation);

        }

        private static async Task CreateNewConsolidatedRecordAsync(CloudTable consolidatedTable, EmployeeEntity employeeEntity, int minutes)
        {
            string[] date = employeeEntity.Date.ToString().Split(" ");
            ConsolidatedEntity consolidatedEntity = new ConsolidatedEntity
            {
                EmployeeId = employeeEntity.EmployeeId,
                Date = Convert.ToDateTime(date[0]),
                MinutesWorked = minutes,
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
            };

            TableOperation addOperation = TableOperation.Insert(consolidatedEntity);
            await consolidatedTable.ExecuteAsync(addOperation);

        }

        private static async Task<List<ConsolidatedEntity>> GetConsolidatedByEmployeeIdAsync(int employeeId, CloudTable consolidatedTable)
        {
            string consolidatedFilter = TableQuery.GenerateFilterConditionForInt("EmployeeId", QueryComparisons.Equal, employeeId);
            TableQuery<ConsolidatedEntity> query = new TableQuery<ConsolidatedEntity>().Where(consolidatedFilter);
            TableQuerySegment<ConsolidatedEntity> consolidatedMatch = await consolidatedTable.ExecuteQuerySegmentedAsync(query, null);

            return consolidatedMatch.Results;
        }
    }
}