using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webHttpTest.Models;

namespace webHttpTest.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HttpRequestViewModel();

            model.Method = "GET";
            model.RequestContentType = "application/json";

            return View(model);
        }

        public IActionResult About()
        {
            string hostName1 = System.Environment.MachineName;

            ViewData.Add("MachineName", System.Environment.MachineName);
            ViewData.Add("HostName", System.Net.Dns.GetHostName());
            ViewData.Add("IpAddresses", GetAllLocalIPv4());
            ViewData.Add("EnvironmentVariables", GetEnvironmentVariables());

            return View();
        }

        public IActionResult TraceRt(TraceRtViewModel viewModel)
        {
            viewModel.Results = new List<PingResult>();

            if (string.IsNullOrEmpty(viewModel.DestinationHost))
            {
                return View("TraceRt", viewModel);
            }

            try
            {
                var hostName = viewModel.DestinationHost;

                var routes = GetTraceRoute(hostName);

                int i = 0;

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

            return View("TraceRt", viewModel);
        }

        public async Task<IActionResult> SendRequest(HttpRequestViewModel viewModel)
        {
            try
            {
                if (!string.IsNullOrEmpty(viewModel.Url))
                {
                    var httpRequest = new HttpRequestMessage();

                    httpRequest.RequestUri = new Uri(viewModel.Url);

                    // request headers
                    if (!string.IsNullOrEmpty(viewModel.RequestHeaders))
                    {
                        var headerList = viewModel.RequestHeaders.Split(Environment.NewLine).ToList<string>();

                        foreach (var headerString in headerList)
                        {
                            var headerArray = headerString.Split(":");

                            if (headerArray.Length == 2)
                            {
                                httpRequest.Headers.Add(headerArray[0].Trim(), headerArray[1].Trim());
                            }
                        }
                    }

                    // method
                    switch (viewModel.Method)
                    {
                        case "GET":
                            httpRequest.Method = HttpMethod.Get;
                            break;
                        case "POST":
                            httpRequest.Method = HttpMethod.Post;
                            break;
                        case "PUT":
                            httpRequest.Method = HttpMethod.Put;
                            break;
                        case "PATCH":
                            httpRequest.Method = HttpMethod.Patch;
                            break;
                        case "DELETE":
                            httpRequest.Method = HttpMethod.Delete;
                            break;
                        default:
                            throw new Exception($"Unknown HTTP Method - {viewModel.Method}");
                    }

                    // body
                    if (!string.IsNullOrEmpty(viewModel.RequestBody))
                    {
                        httpRequest.Content = new StringContent(viewModel.RequestBody,
                                        Encoding.UTF8,
                                        viewModel.RequestContentType);
                    }

                    var httpClient = new HttpClient();

                    var stopwatch = new Stopwatch();

                    stopwatch.Start();

                    var result = await httpClient.SendAsync(httpRequest);

                    var response = await result.Content.ReadAsStringAsync();

                    stopwatch.Stop();

                    // Response properties
                    viewModel.ResponseBody = PrettyPrint(response);

                    viewModel.ResponseCode = (int)result.StatusCode;
                    viewModel.ResponseCodeText = result.StatusCode.ToString();
                    viewModel.ResponseBodyLength = result.Content.Headers.ContentLength.Value;
                    viewModel.ResponseContentType = result.Content.Headers.ContentType == null ? "Unkown" : result.Content.Headers.ContentType.ToString();
                    viewModel.ResponseTimeMilliseconds = stopwatch.ElapsedMilliseconds;

                    // Headers 

                    viewModel.ResponseHeaders = new List<Header>();

                    foreach (var header in result.Headers)
                    {
                        viewModel.ResponseHeaders.Add(new Header { Key = header.Key, Value = header.Value.First() });
                    }
                }
            }
            catch (Exception ex)
            {
                viewModel.ErrorText = ex.ToString();

                if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
                {
                    viewModel.ErrorText += $"{Environment.NewLine}{ex.InnerException}";
                }
            }

            return View("Index", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

        public string PrettyPrint(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            input = Regex.Replace(input, $"({Environment.NewLine})+", $"{Environment.NewLine}")
                        .Trim();

            try
            {
                return XDocument.Parse(input).ToString();

            }
            catch (Exception) { }

            try
            {
                var t = JsonConvert.DeserializeObject<object>(input);
                return JsonConvert.SerializeObject(t, Formatting.Indented);
            }
            catch (Exception) { }

            return input;
        }

        // https://stackoverflow.com/a/45565253
        private IEnumerable<PingResult> GetTraceRoute(string hostname, int timeout = 10000, int maxTTL = 30, int bufferSize = 32)
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

                if (reply1.Status == IPStatus.TtlExpired)
                {
                    // TtlExpired means we've found an address, but there are more addresses
                    pingResult.iPAddress = reply1.Address;
                    pingResult.hostName = GetHostNameFromIp(pingResult.iPAddress);

                    yield return pingResult;
                    continue;
                }
                if (reply1.Status == IPStatus.TimedOut)
                {
                    // TimedOut means this ttl is no good, we should continue searching
                    yield return pingResult;
                    continue;
                }
                if (reply1.Status == IPStatus.Success)
                {
                    // Success means the tracert has completed
                    pingResult.iPAddress = reply1.Address;
                    pingResult.hostName = GetHostNameFromIp(pingResult.iPAddress);
                }

                // if we ever reach here, we're finished, so break
                yield return pingResult;
                break;
            }

        }

        private List<string> GetAllLocalIPv4()
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

        private Dictionary<string, string> GetEnvironmentVariables()
        {
            var envVariables = new Dictionary<string, string>();

            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                envVariables.Add((string)item.Key, (string)item.Value);
            }

            return envVariables;
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
