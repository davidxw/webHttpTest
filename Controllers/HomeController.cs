using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
            return View();
        }

        public IActionResult TraceRt(TraceRtViewModel viewModel)
        {
            viewModel.Results = new List<string>();

            if (string.IsNullOrEmpty(viewModel.DestinationHost))
            {
                return View("TraceRt", viewModel);
            }

            try
            {
                var hostName = viewModel.DestinationHost;

                var i = 0;

                var routes = GetTraceRoute(hostName);

                foreach (var ip in routes)
                {

                    i++;

                    if (ip == null)
                    {
                        viewModel.Results.Add($"{i}. Request timed out");
                        continue;
                    }

                    var hostString = string.Empty;

                    try
                    {
                        var host = Dns.GetHostEntry(ip);
                        hostString = host.HostName;
                    }
                    catch (Exception) { }

                    var resultString = string.Empty;

                    if (string.IsNullOrEmpty(hostString))
                    {
                        resultString = $"{i}. {ip}";
                    }
                    else
                    {
                        resultString = $"{i}. {hostString} [{ip}]";
                    }

                    Debug.WriteLine(resultString);
                    
                    viewModel.Results.Add(resultString);

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
                    viewModel.ResponseContentType = result.Content.Headers.ContentType.ToString();
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
                viewModel.ErrorText = ex.Message;

                if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
                {
                    viewModel.ErrorText += $"{Environment.NewLine}{ex.InnerException.Message}";
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
        public static IEnumerable<IPAddress> GetTraceRoute(string hostname)
        {
            // following are the defaults for the "traceroute" command in unix.
            const int timeout = 10000;
            const int maxTTL = 30;
            const int bufferSize = 32;

            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            Ping pinger = new Ping();

            for (int ttl = 1; ttl <= maxTTL; ttl++)
            {
                PingOptions options = new PingOptions(ttl, true);
                PingReply reply = pinger.Send(hostname, timeout, buffer, options);

                if (reply.Status == IPStatus.TtlExpired)
                {
                    // TtlExpired means we've found an address, but there are more addresses
                    yield return reply.Address;
                    continue;
                }
                if (reply.Status == IPStatus.TimedOut)
                {
                    // TimedOut means this ttl is no good, we should continue searching
                    continue;
                }
                if (reply.Status == IPStatus.Success)
                {
                    // Success means the tracert has completed
                    yield return reply.Address;
                }

                // if we ever reach here, we're finished, so break
                break;
            }
        }
    }
}
