using System.Collections.Generic;
using webHttpTest.Models;

namespace webHttpTest.Services
{
    public interface INetworkService
    {
        List<string> GetAllLocalIPv4();
        IEnumerable<PingResult> GetTraceRoute(string hostname, int timeout = 10000, int maxTTL = 30, int bufferSize = 32);
    }
}