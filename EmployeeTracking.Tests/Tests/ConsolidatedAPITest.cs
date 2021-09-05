using EmployeeTracking.Common.Models;
using EmployeeTracking.Functions.Functions;
using EmployeeTracking.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EmployeeTracking.Tests.Tests
{
    public class ConsolidatedAPITest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void Consolidated_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEmployees = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Consolidated consolidatedRequest = TestFactory.GetConsolidatedRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestConsolidated(consolidatedRequest);

            //Act
            IActionResult response = await ConsolidatedAPI.Consolidated(request, mockConsolidated ,mockEmployees, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetConsolidatedRecordsByDate_Should_Return_200()
        {
            //Arrange
            MockCloudTableConsolidated mockConsolidated = new MockCloudTableConsolidated(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Consolidated consolidatedRequest = TestFactory.GetConsolidatedRequest();
            DateTime date = DateTime.UtcNow;
            DefaultHttpRequest request = TestFactory.CreateHttpRequestConsolidated(consolidatedRequest);

            //Act
            IActionResult response = await ConsolidatedAPI.GetConsolidatedRecordsByDate(request, mockConsolidated, date, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

    }
}
