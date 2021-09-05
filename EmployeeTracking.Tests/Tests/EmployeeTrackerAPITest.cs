using EmployeeTracking.Common.Models;
using EmployeeTracking.Functions.Entities;
using EmployeeTracking.Functions.Functions;
using EmployeeTracking.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace EmployeeTracking.Tests.Tests
{
    public class EmployeeTrackerAPITest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateEntry_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEntries = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Employee employeeRequest = TestFactory.GetEmployeeRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestEmployee(employeeRequest);

            //Act
            IActionResult response = await EmployeeTrackerAPI.CreateEntry(request, mockEntries, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void UpdateEntry_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEntries = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Employee employeeRequest = TestFactory.GetEmployeeRequest();
            Guid employeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestEmployee(employeeId, employeeRequest);

            //Act
            IActionResult response = await EmployeeTrackerAPI.UpdateEntry(request, mockEntries, employeeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllEmployeeEntries_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEntries = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Employee employeeRequest = TestFactory.GetEmployeeRequest();
            Guid employeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestEmployee();

            //Act
            IActionResult response = await EmployeeTrackerAPI.GetAllEmployeeEntries(request, mockEntries, logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public void GetEntryById_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEntries = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeEntity employeeEntity = TestFactory.GetEmployeeEntity();
            Guid employeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestEmployee();

            //Act
            IActionResult response = EmployeeTrackerAPI.GetEntryById(request, employeeEntity, mockEntries, employeeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void DeleteEntry_Should_Return_200()
        {
            //Arrange
            MockCloudTableTime mockEntries = new MockCloudTableTime(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            EmployeeEntity employeeEntity = TestFactory.GetEmployeeEntity();
            Guid employeeId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequestEmployee();

            //Act
            IActionResult response = await EmployeeTrackerAPI.DeleteEntry(request,employeeEntity, mockEntries, employeeId.ToString(), logger);

            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
