using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.Model
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class EndpointAttribute : Attribute
    {
        public string EndpointTemplate { get; }


        public EndpointAttribute(string endpointTemplate)
        {
            EndpointTemplate = endpointTemplate;
        }
    }
}
