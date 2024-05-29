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
    }


}
