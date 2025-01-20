using KepwareSync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KepwareSync.Serializer
{
    public class JsonProjectRoot
    {
        [JsonPropertyName("project")]
        public Project? Project { get; set; }
    }
}
