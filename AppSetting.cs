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

        public AppSetting(string webSocketUrl, string apiUrl)
        {
            WebSocketUrl = webSocketUrl;
            ApiUrl = apiUrl;
        }
    }
}
