using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EmployeeTracking.Tests.Helpers
{
    public class MockCloudTableTime : CloudTable
    {
        public MockCloudTableTime(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableTime(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableTime(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }

        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetEmployeeEntity()
            });
        }

        public override async Task<TableQuerySegment<EmployeeEntity>> ExecuteQuerySegmentedAsync<EmployeeEntity>(TableQuery<EmployeeEntity> query, TableContinuationToken token)
        {
            ConstructorInfo constructor = typeof(TableQuerySegment<EmployeeEntity>)
                   .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                   .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return await Task.FromResult(constructor.Invoke(new object[] { TestFactory.GetEmployeeEntities() })
                                         as TableQuerySegment<EmployeeEntity>);
        }
    }
}
