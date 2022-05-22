using System;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Poly.Rest
{
    [Serializable]
    public struct RestResponse<T>
    {
        public int Code;
        public string Status;
        public T Data;
    }
    public static class RestUtil
    {
        private static JsonSerializer serializer;// = JsonSerializer.CreateDefault();

        public static readonly int Code_NeedAuthority = 1010;
        public static readonly string Error_NeedAuthority = "Need authority";

        public static readonly int Code_NoValue = 1100;
        public static readonly string Error_NoValue = "No value";

        public static readonly int Code_OperationFailed = 1101;
        public static readonly string Error_OperationFailed = "Operation failed";

        static RestUtil()
        {
            var settings = JsonConvert.DefaultSettings?.Invoke();
            settings ??= new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            serializer = JsonSerializer.Create(settings);
        }

        //public static object Deserialize(this JsonSerializer serializer, Stream stream, Type objectType)
        //{
        //    var reader = new StreamReader(stream);
        //    var data = serializer.Deserialize(reader, objectType);
        //    return data;
        //}
        public static object Deserialize(Stream stream, Type objectType)
        {
            var reader = new StreamReader(stream);
            var data = serializer.Deserialize(reader, objectType);
            return data;
        }
        public static T Deserialize<T>(Stream stream)
        {
            return (T)Deserialize(stream, typeof(T));
        }

        public static RestResponse<T> Deserialize<T>(string json)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            return serializer.Deserialize<RestResponse<T>>(reader);
        }

        //public static string Serialize<T>(int code, string status, T data)
        //{
        //    return Serialize<T>(new RestResponse<T>
        //    {
        //        Code = code, Status = status, Data = data
        //    });
        //}
        public static string Serialize<T>(RestResponse<T> response)
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, response);
            }
            return sb.ToString();
        }

        public static async Task SendResponseAsync(this HttpListenerResponse response, byte[] contents)
        {
            try
            {
                var length = contents.Length;
                response.ContentLength64 = length;
                await response.OutputStream.WriteAsync(contents, 0, length);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e.Message}");
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        public static async Task SendResponseAsync(this HttpListenerResponse response, string text, string mimeType = "text/plain")
        {
            response.ContentType = mimeType;
            // response.AddHeader("content-type", "application/json");
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            await response.SendResponseAsync(bytes);
        }
        public static async Task SendJsonResponseAsync(this HttpListenerResponse response, string text)
        {
            await response.SendResponseAsync(text, "application/json");
        }

        public static string GenerateToken()
        {
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            return token;
        }
        public static bool IsTokenExpired(string token, float hour)
        {
            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (when < DateTime.UtcNow.AddHours(-hour))
            {
                // too old
                return false;
            }
            return true;
        }
    }
}