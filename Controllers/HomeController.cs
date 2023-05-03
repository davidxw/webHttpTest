using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using webHttpTest.Hubs;
using webHttpTest.Models;
using webHttpTest.Services;

namespace webHttpTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<TraceRtHub> _hubContext;
        private INetworkService _networkService;

        public HomeController(INetworkService networkService, IHubContext<TraceRtHub> hubContext)
        {
            _networkService = networkService;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            var model = new HttpRequestViewModel();

            model.Method = "GET";
            model.RequestContentType = "application/json";

            return View(model);
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

                    var httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                    {

                        if (sslPolicyErrors != SslPolicyErrors.None)
                        {
                            viewModel.ErrorText = $"Warning - SSL policy errors: {sslPolicyErrors}";
                        }

                        return true;
                    };

                    var httpClient = new HttpClient(httpClientHandler);

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



    }
}
