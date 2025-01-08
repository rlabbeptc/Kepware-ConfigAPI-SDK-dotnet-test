using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    public class EntityCollection<T> where T : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public BaseEntity? Owner { get; }

        [JsonIgnore]
        [YamlIgnore]
        public List<T> Items { get; set; } = new();

        public EntityCollection(BaseEntity? owner)
        {
            Owner = owner;
        }

        public void SetOwnerForItems()
        {
            foreach (var item in Items)
            {
                if (item is IHaveOwner ownable)
                {
                    ownable.Owner = Owner;
                }
            }
        }
    }
}
