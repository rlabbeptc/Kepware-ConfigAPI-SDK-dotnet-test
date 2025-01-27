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
    public abstract class EntityCollection<T> : List<T>, IHaveOwner
        where T : BaseEntity
    {
        [JsonIgnore]
        [YamlIgnore]
        public NamedEntity? Owner { get; set; }

        protected EntityCollection()
        {
            
        }

        protected EntityCollection(IEnumerable<T> collection)
            : base(collection)
        {
            
        }

        //public IEnumerator<T> GetEnumerator()
        //=> Items.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator()
        //=> (Items as IEnumerable).GetEnumerator();
    }
}
