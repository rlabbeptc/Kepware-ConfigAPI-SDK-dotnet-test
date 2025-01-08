using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    public interface IHaveOwner
    {
        [JsonIgnore]
        [YamlIgnore]
        public BaseEntity? Owner { get; set; }
    }

    public abstract class BaseEntity
    {
        [JsonPropertyName("common.ALLTYPES_NAME")]
        [YamlMember(Alias = "common.ALLTYPES_NAME")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("common.ALLTYPES_DESCRIPTION")]
        [YamlMember(Alias = "common.ALLTYPES_DESCRIPTION")]
        public string Description { get; set; } = string.Empty;

        [JsonExtensionData]
        [YamlIgnore]
        public Dictionary<string, object> DynamicProperties { get; set; } = new();
    }

    public class DefaultEntity(BaseEntity owner) : BaseEntity, IHaveOwner
    {
        public BaseEntity? Owner { get; set; } = owner;
    }
}
