using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using webHttpTest.Models;

namespace webHttpTest.Services
{
    public class HostingEnvironmentService : IHostingEnvironmentService
    {
        private INetworkService _networkService;

        public HostingEnvironmentService(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public string PrintHostingEnvironment()
        {
            var hostingEnvironment = GetHostingEnvironment();

            return JsonConvert.SerializeObject(hostingEnvironment, Formatting.Indented);
        }

        public HostingEnvironment GetHostingEnvironment()
        {
            var environment = new HostingEnvironment();

            environment.MachineName = System.Environment.MachineName;
            environment.HostName = System.Net.Dns.GetHostName();
            environment.IpAddresses = _networkService.GetAllLocalIPv4();
            environment.EnvironmentVariables = GetEnvironmentVariables();

            return environment;
        }

        private Dictionary<string, string> GetEnvironmentVariables()
        {
            var envVariables = new Dictionary<string, string>();

            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                envVariables.Add((string)item.Key, (string)item.Value);
            }

            return envVariables;
        }
    }
}
