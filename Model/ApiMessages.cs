using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KepwareSync.Model
{
    public class ApiResult
    {

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonIgnore]
        public HttpStatusCode HttpStatusCode => (HttpStatusCode)Code;


        /// <summary>
        /// true if System.Net.Http.HttpResponseMessage.StatusCode was in the range 200-299
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessStatusCode => Code >= 200 && Code < 300;
    }

    
}
