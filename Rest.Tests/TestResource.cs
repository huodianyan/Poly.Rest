using System;
using System.Net;
using System.Threading.Tasks;

namespace Poly.Rest.Tests
{
    [Serializable]
    public class GetKVRequest
    {
        public string Key;
    }
    [Serializable]
    public class GetKVResponse
    {
        public string Value;
    }

    [RestResource(BasePath = "/Test")]
    public class TestResource
    {
        [RestRoute(ERestMethod.POST, "/GetKV")]
        public async Task GetKV(HttpListenerContext context)
        {
            var token = context.Request.Headers.Get("X-Authorization");
            var restResp = new RestResponse<GetKVResponse>();
            try
            {
                var req = RestUtil.Deserialize<GetKVRequest>(context.Request.InputStream);

                var key = req.Key;
                if (key == null)
                {
                    restResp.Code = RestUtil.Code_NoValue;
                    restResp.Status = RestUtil.Error_NoValue;
                }
                else
                {
                    var resp = new GetKVResponse();
                    resp.Value = $"{key}_value";
                    restResp.Data = resp;
                    restResp.Code = 200;
                }
            }
            catch (Exception ex)
            {
                restResp.Code = RestUtil.Code_OperationFailed;
                restResp.Status = ex.Message;
            }
            var json = RestUtil.Serialize(restResp);
            //Console.WriteLine($"GetKV: {json}");
            await context.Response.SendJsonResponseAsync(json);
        }
    }
}