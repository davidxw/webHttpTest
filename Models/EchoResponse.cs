namespace webHttpTest.Models
{
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class EchoResponse
    {
        public string Method { get; set; }
        public string Host { get; set; }
        public PathString Path { get; set; }
        public QueryString QueryString { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public dynamic Body { get; set; }
        //public IQueryCollection QueryParams { get; set; }

    }

    public static class ExtensionMethods
    {
        public static async Task<EchoResponse> ToEchoResponseAsync(this HttpRequest request)
        {
            var echoResponse = new EchoResponse();
            using (StreamReader streamReader = new StreamReader(request.Body))
            {
                echoResponse.Body = await streamReader.ReadToEndAsync();
            }
            ;

            // attempt to transform reponse to JSON
            try
            {
                echoResponse.Body = JsonSerializer.Deserialize<dynamic>(echoResponse.Body);
            }
            catch
            {
                // do nothing
            }

            echoResponse.Host = request.Host.Value;
            echoResponse.Method = request.Method;
            echoResponse.Headers = request.Headers;
            echoResponse.Path = request.Path;
            echoResponse.QueryString = request.QueryString;
            //echoResponse.QueryParams = request.Query;

            return echoResponse;
        }
    }
}
