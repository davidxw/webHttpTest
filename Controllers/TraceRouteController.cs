using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webHttpTest.Hubs;
using webHttpTest.Models;
using webHttpTest.Services;

namespace webHttpTest.Controllers
{
    public class TraceRouteController : Controller
    {
        private INetworkService _networkService;

        public TraceRouteController(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public IActionResult Index(TraceRtViewModel viewModel)
        {
            viewModel.Results = new List<PingResult>();

            if (string.IsNullOrEmpty(viewModel.DestinationHost))
            {
                return View("Index", viewModel);
            }

            try
            {
                var hostName = viewModel.DestinationHost;

                var routes = _networkService.GetTraceRoute(hostName);

                int i = 1;

                foreach (var pingResult in routes)
                {
                    pingResult.hop = i++;
                    viewModel.Results.Add(pingResult);

                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorText = ex.Message;

                if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
                {
                    viewModel.ErrorText += $"{Environment.NewLine}{ex.InnerException.Message}";
                }
            }

            return View("Index", viewModel);
        }

        public IActionResult AsyncIndex()
        {
            return View();
        }
    }
}
