using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamsFiles
{
    internal class AppSetting
    {
        public string WebSocketUrl { get; set; }
        public string ApiUrl { get; set; }
        public string UploadEndpointChunk { get; } = "/uploadChunk";
        public string UploadEndpointSaveFile { get; } = "/saveFile";
        private static Random random = new Random();

        public AppSetting(string webSocketUrl, string apiUrl)
        {
            WebSocketUrl = webSocketUrl;
            ApiUrl = apiUrl;
        }
        public static string RandomString()
        {
            int randomStringInt = 10;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, randomStringInt)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
