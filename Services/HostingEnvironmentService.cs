using Azure.Identity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using webHttpTest.Models;

namespace webHttpTest.Services
{
    public class HostingEnvironmentService : IHostingEnvironmentService
    {
        private INetworkService _networkService;

        public HostingEnvironmentService(INetworkService networkService)
        {
            _networkService = networkService;
        }

        public string PrintHostingEnvironment()
        {
            var hostingEnvironment = GetHostingEnvironment();

            return JsonConvert.SerializeObject(hostingEnvironment, Formatting.Indented);
        }

        public HostingEnvironment GetHostingEnvironment()
        {
            var environment = new HostingEnvironment();

            environment.MachineName = System.Environment.MachineName;
            environment.HostName = System.Net.Dns.GetHostName();
            environment.IpAddresses = _networkService.GetAllLocalIPv4();
            environment.EnvironmentVariables = GetEnvironmentVariables();
            environment.ProcessorCount = Environment.ProcessorCount;

            var azureIndentityResult = GetAzureDefaultIdentity().Result;

            if (azureIndentityResult != null)
            {
                environment.AzureIdentity = azureIndentityResult.ToString();
            }
            else
            {
                environment.AzureIdentity = "No Azure Identity Found";
            }

            return environment;
        }

        private Dictionary<string, string> GetEnvironmentVariables()
        {
            var envVariables = new SortedDictionary<string, string>();

            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                envVariables.Add((string)item.Key, (string)item.Value);
            }

            return envVariables.ToDictionary<string, string>();
        }

        private async Task<JwtSecurityToken> GetAzureDefaultIdentity()
        {
            try
            {
                var credential = new DefaultAzureCredential();
                string[] scopes = new string[] { "https://graph.microsoft.com/.default" };
                var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes));

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token.Token) as JwtSecurityToken;
                //var upn = jsonToken.Claims.First(c => c.Type == "upn").Value;

                return jsonToken;
            }
            catch
            {
                return null;
            }
        }
    }
}
