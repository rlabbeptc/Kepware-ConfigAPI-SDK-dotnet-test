using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kepware.Api.Model
{
    public class EntityCollection<T> : List<T>, IHaveOwner
        where T : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public NamedEntity? Owner { get; set; }

        public EntityCollection()
        {

        }

        public EntityCollection(IEnumerable<T> collection)
            : base(collection)
        {

        }
    }
}
