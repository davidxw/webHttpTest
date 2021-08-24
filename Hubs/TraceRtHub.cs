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

            try
            {
                var routes = _networkService.GetTraceRoute(hostName);

                int i = 1;

                foreach (var pingResult in routes)
                {
                    pingResult.hop = i++;

                    var pingResultJson = JsonConvert.SerializeObject(pingResult);
                    await _hubContext.Clients.All.SendAsync("Notify", pingResultJson);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}