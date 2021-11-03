using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webHttpTest.Hubs;
using webHttpTest.Models;
using webHttpTest.Services;

namespace webHttpTest.Controllers
{
    public class AboutController : Controller
    {
        private IHostingEnvironmentService _hostingEnvironmentService;

        public AboutController(IHostingEnvironmentService hostingEnvironmentService)
        {
            _hostingEnvironmentService = hostingEnvironmentService;
        }

        public IActionResult Index()
        {
            var environment = _hostingEnvironmentService.GetHostingEnvironment();

            return View(environment);
        }

        private static Dictionary<string, string> GetEnvironmentVariables()
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
