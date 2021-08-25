using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webHttpTest.Hubs;
using webHttpTest.Services;

namespace webHttpTest.Controllers
{
    public class AboutController : Controller
    {
        private INetworkService _networkService;

        public AboutController(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public IActionResult Index()
        {
            string hostName1 = System.Environment.MachineName;

            ViewData.Add("MachineName", System.Environment.MachineName);
            ViewData.Add("HostName", System.Net.Dns.GetHostName());
            ViewData.Add("IpAddresses", _networkService.GetAllLocalIPv4());
            ViewData.Add("EnvironmentVariables", GetEnvironmentVariables());

            return View();
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
