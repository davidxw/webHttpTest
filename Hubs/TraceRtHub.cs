using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using webHttpTest.Models;
using webHttpTest.Services;
using Newtonsoft.Json;
using System.Diagnostics;

namespace webHttpTest.Hubs
{
    public class TraceRtHub : Hub
    {
        private readonly IHubContext<TraceRtHub> _hubContext;
        private INetworkService _networkService;

        public TraceRtHub(INetworkService networkService, IHubContext<TraceRtHub> hubContext)
        {
            _hubContext = hubContext;
            _networkService = networkService;
        }

        public async Task StartTrace(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
            {
                return;
            }

            // message types:
            //  hop - trace hop details
            //  end - no more messages expected
            //  error - error message to display

            try
            {
                var routes = _networkService.GetTraceRoute(hostName);

                int i = 1;

                foreach (var pingResult in routes)
                {
                    pingResult.hop = i++;

                    var pingResultJson = JsonConvert.SerializeObject(pingResult);
                    await _hubContext.Clients.All.SendAsync("Notify", "hop", pingResultJson);
                }

                await _hubContext.Clients.All.SendAsync("Notify", "end");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;

                if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
                {
                    errorMessage += $"{Environment.NewLine}{ex.InnerException.Message}";
                }


                await _hubContext.Clients.All.SendAsync("Notify", "error", $"{errorMessage}");
            }
        }
    }
}