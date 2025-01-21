using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kepware.Api.Serializer
{
    public class JsonProjectRoot
    {
        [JsonPropertyName("project")]
        public Project? Project { get; set; }
    }
}
