# Poly.Rest
a lightweight Rest Server/Client service for Unity and any C# (or .Net) project.

Function and interface refer to [grapevine](https://github.com/scottoffen/grapevine).

## Features
- Zero dependencies (temporarily use Json.Net for serialization)
- Minimal core (< 1000 lines)
- Lightweight and fast
- Register RestResource/RestRoute to handle rest request
- Adapted to all C# game engine

## Installation

## Overview

```csharp

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
        await context.Response.SendJsonResponseAsync(json);
    }
}

var routingManager = new RestRoutingManager();
routingManager.RegisterResource("Poly.Rest.Tests");
var restServer = new RestServer(1234, null, routingManager);
restServer.Start();

restClient = new RestClient("http://localhost:1234/", 3f);
var restResponse = await restClient.CallRestAPIAsync<GetKVRequest, GetKVResponse>("Test/GetKV", request);
restClient.Dispose();

restServer.Stop();
routingManager.Dispose();

```

## License
The software is released under the terms of the [MIT license](./LICENSE.md).

## FAQ

## References

### Documents

### Projects
- [scottoffen/grapevine](https://github.com/scottoffen/grapevine)

### Benchmarks
