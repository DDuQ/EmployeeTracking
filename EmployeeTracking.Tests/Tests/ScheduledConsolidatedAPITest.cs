using EmployeeTracking.Common.Models;
using EmployeeTracking.Functions.Functions;
using EmployeeTracking.Tests.Helpers;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmployeeTracking.Tests.Tests
{
    public class ScheduledConsolidatedAPITest
    {
        [Fact]
        public async Task ScheduledConsolidated_Should_Return_200()
        {
            //Arrange
            TimerInfo myTimer = default(TimerInfo);
            MockCloudTableTime mockEmployees = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Consolidated consolidatedRequest = TestFactory.GetConsolidatedRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestConsolidated(consolidatedRequest);
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);

            //Act
            await ScheduledConsolidatedAPI.Run(myTimer, mockConsolidated, mockEmployees, logger);
            string message = logger.Logs[0];

            //Assert
            Assert.Contains("Time scheduled", message);
        }
    }
}
