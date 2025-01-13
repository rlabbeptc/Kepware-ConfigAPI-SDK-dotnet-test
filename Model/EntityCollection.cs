using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    public abstract class EntityCollection<T> : IHaveOwner, IEnumerable<T>
        where T : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public BaseEntity? Owner { get; set; }

        public List<T> Items { get; set; } = new();

        public IEnumerator<T> GetEnumerator()
        => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => (Items as IEnumerable).GetEnumerator();
    }
}
