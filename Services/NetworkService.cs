using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using webHttpTest.Models;

namespace webHttpTest.Services
{
    public class NetworkService : INetworkService
    {
        // https://stackoverflow.com/a/45565253
        public IEnumerable<PingResult> GetTraceRoute(string hostname, int timeout = 10000, int maxTTL = 30, int bufferSize = 32)
        {
            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            Ping pinger = new Ping();

            for (int ttl = 1; ttl <= maxTTL; ttl++)
            {
                var pingResult = new PingResult();

                PingOptions options = new PingOptions(ttl, true);

                var st = new Stopwatch();

                st.Start();
                PingReply reply1 = pinger.Send(hostname, timeout, buffer, options);
                st.Stop();

                pingResult.et1 = st.Elapsed.Milliseconds;

                st.Restart();
                PingReply reply2 = pinger.Send(hostname, timeout, buffer, options);
                st.Stop();

                pingResult.et2 = st.Elapsed.Milliseconds;

                st.Restart();
                PingReply reply3 = pinger.Send(hostname, timeout, buffer, options);
                st.Stop();

                pingResult.et3 = st.Elapsed.Milliseconds;

                if (reply1.Status == IPStatus.TtlExpired || reply2.Status == IPStatus.TtlExpired || reply3.Status == IPStatus.TtlExpired)
                {
                    // TtlExpired means we've found an address, but there are more addresses
                    var ip = reply1.Status == IPStatus.TtlExpired ? reply1.Address : reply2.Status == IPStatus.TtlExpired ? reply2.Address : reply3.Address;
                    pingResult.iPAddress = ip.ToString();
                    pingResult.hostName = GetHostNameFromIp(ip);

                    yield return pingResult;
                    continue;
                }
                if (reply1.Status == IPStatus.TimedOut && reply2.Status == IPStatus.TimedOut && reply3.Status == IPStatus.TimedOut)
                {
                    // TimedOut means this ttl is no good, we should continue searching
                    yield return pingResult;
                    continue;
                }
                if (reply1.Status == IPStatus.Success || reply2.Status == IPStatus.Success || reply3.Status == IPStatus.Success)
                {
                    // Success means the tracert has completed
                    var ip = reply1.Status == IPStatus.Success ? reply1.Address : reply2.Status == IPStatus.Success ? reply2.Address : reply3.Address;
                    pingResult.iPAddress = ip.ToString();
                    pingResult.hostName = GetHostNameFromIp(ip);
                }

                // if we ever reach here, we're finished, so break
                yield return pingResult;
                break;
            }

        }

        public List<string> GetAllLocalIPv4()
        {
            // https://stackoverflow.com/questions/6803073/get-local-ip-address

            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add($"{ip.Address} ({item.NetworkInterfaceType})");
                        }
                    }
                }
            }
            return ipAddrList;
        }
        private string GetHostNameFromIp(IPAddress iPAddress)
        {
            var hostString = string.Empty;

            try
            {
                var host = Dns.GetHostEntry(iPAddress);
                hostString = host.HostName;
            }
            catch (Exception) { }

            return hostString;
        }
    }
}
