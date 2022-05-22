using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Poly.Rest
{
    public interface IRestClient : IDisposable
    {
        string Token { get; set; }
        string BaseUrl { get; }

        Task<RestResponse<TResp>> CallRestAPIAsync<TReq, TResp>(string api, TReq req, ERestMethod method = ERestMethod.POST);
    }
    public class RestClient : IRestClient
    {
        private HttpClient client;
        private string token;
        //private readonly float timeout = 10;
        private readonly string baseUrl;

        public string Token { get => token; set => token = value; }
        public string BaseUrl => baseUrl;

        public RestClient(string url, float timeout = 10)
        {
            client = new HttpClient();
            baseUrl = string.IsNullOrWhiteSpace(url) ? "" : url.TrimEnd('/');
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }
        public void Dispose()
        {
            client.Dispose();
            client = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private HttpMethod ToHttpMethod(ERestMethod method)
        {
            switch(method)
            {
                case ERestMethod.DELETE: return HttpMethod.Delete;
                case ERestMethod.POST: return HttpMethod.Post;
                case ERestMethod.PUT: return HttpMethod.Put;
                case ERestMethod.GET:
                default:
                    return HttpMethod.Get;
            }
        }
        public async Task<RestResponse<TResp>> CallRestAPIAsync<TReq, TResp>(string api, TReq req, ERestMethod method = ERestMethod.POST)
        {
            var request = new HttpRequestMessage();

            //headers
            if (token != null)
                request.Headers.Add("X-Authorization", token);
            //method
            request.Method = ToHttpMethod(method);
            //content
            var json = JsonConvert.SerializeObject(req);
            request.Content = new StringContent(json);
            if (request.Content is MultipartContent) request.Headers.ExpectContinue = false;
            //requesturi
            var apiUrl = $"{baseUrl}/{api.TrimStart('/')}";
            //Console.WriteLine($"CallRestAPIAsync: {apiUrl}");
            request.RequestUri = new Uri(apiUrl);

            var cancleToken = CancellationToken.None;
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancleToken).ConfigureAwait(false);

            RestResponse<TResp> restResp = default;
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine(response.StatusCode.ToString());
                //resp = new RestResponse();
                restResp.Code = (int)response.StatusCode;
                restResp.Status = response.StatusCode.ToString();
            }
            else
            {
                if (response.Content != null)
                    json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                else
                    json = response.StatusCode.ToString();
                //Debug.Log($"VRCellRestAPI.CallRestAPIAsync: resp {json}");
                restResp = RestUtil.Deserialize<TResp>(json);
            }
            return restResp;
        }
    }
}