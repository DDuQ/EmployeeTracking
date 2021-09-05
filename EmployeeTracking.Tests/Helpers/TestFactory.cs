using EmployeeTracking.Common.Models;
using EmployeeTracking.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmployeeTracking.Tests.Helpers
{
    public class TestFactory
    {
        public static EmployeeEntity GetEmployeeEntity()
        {
            return new EmployeeEntity
            {
                ETag = "*",
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                IsConsolidated = false,
                EmployeeId = 1,
                Type = 0
            };
        }

        public static List<EmployeeEntity> GetEmployeeEntities()
        {
            return new List<EmployeeEntity>();
        }

        public static ConsolidatedEntity GetConsolidatedEntity()
        {
            return new ConsolidatedEntity
            {
                ETag = "*",
                PartitionKey = "CONSOLIDATED",
                RowKey = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                MinutesWorked = 0,
                EmployeeId = 1,
            };
        }

        public static DefaultHttpRequest CreateHttpRequestEmployee(Guid employeeId, Employee employeeRequest)
        {
            string request = JsonConvert.SerializeObject(employeeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{employeeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequestEmployee(Guid employeeId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{employeeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequestEmployee(Employee employeeRequest)
        {
            string request = JsonConvert.SerializeObject(employeeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequestEmployee()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Employee GetEmployeeRequest()
        {
            return new Employee
            {
                EmployeeId = 1,
                Date = DateTime.UtcNow,
                IsConsolidated = false,
                Type = 0,
            };
        }

        //Consolidated Requests
        public static DefaultHttpRequest CreateHttpRequestConsolidated(Guid consolidatedId, Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{consolidatedId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequestConsolidated(Guid consolidatedId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{consolidatedId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequestConsolidated(Consolidated consolidatedRequest)
        {
            string request = JsonConvert.SerializeObject(consolidatedRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequestConsolidated()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Consolidated GetConsolidatedRequest()
        {
            return new Consolidated
            {
                EmployeeId = 1,
                Date = DateTime.UtcNow,
                MinutesWorked = 0,
            };
        }

        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if(type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
