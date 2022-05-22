using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Poly.Rest.Tests
{
    [TestClass]
    public partial class RestTest
    {
        private static RestServer restServer;
        private static RestRoutingManager routingManager;
        private IRestClient restClient;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            routingManager = new RestRoutingManager();
            routingManager.RegisterResource("Poly.Rest.Tests");
            //start rest server
            restServer = new RestServer(1234, null, routingManager);
            restServer.Start();
        }
        [ClassCleanup]
        public static void ClassCleanup()
        {
            //stop rest server
            restServer.Stop();
            restServer = null;
            routingManager.Dispose();
            routingManager = null;
        }
        [TestInitialize]
        public void TestInitialize()
        {
            restClient = new RestClient("http://localhost:1234/", 3f);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            restClient.Dispose();
            restClient = null;
        }

        [TestMethod]
        public async Task CallRestTest()
        {
            var request = new GetKVRequest
            {
                Key = "1"
            };
            Assert.AreEqual("1", request.Key);
            var restResponse = await restClient.CallRestAPIAsync<GetKVRequest, GetKVResponse>("Test/GetKV", request);
            Assert.AreEqual(200, restResponse.Code);
            var response = restResponse.Data;
            Assert.AreEqual($"{request.Key}_value", response.Value);
        }
    }
}