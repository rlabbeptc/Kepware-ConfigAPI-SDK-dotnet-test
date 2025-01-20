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
        public string? Suffix { get; } = null;

        public EndpointAttribute(string endpointTemplate, string? suffix = default)
        {
            EndpointTemplate = endpointTemplate;
            Suffix = suffix;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class RecursiveEndpointAttribute : EndpointAttribute
    {
        public string RecursiveEnd { get; }
        public Type RecursiveOwnerType { get; }

        public RecursiveEndpointAttribute(string endpointTemplate, string recursiveEnd, Type recursiveOwnerType, string? suffix = default)
            : base(endpointTemplate, suffix)
        {
            RecursiveEnd = recursiveEnd;
            RecursiveOwnerType = recursiveOwnerType;
        }
    }
}
