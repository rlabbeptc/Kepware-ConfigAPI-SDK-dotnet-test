using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kepware.Api
{
    public interface IKepwareDefaultValueProvider
    {
        public Task<ReadOnlyDictionary<string, JsonElement>> GetDefaultValuesAsync(string driverName, string entityName, CancellationToken cancellationToken = default);

    }
}
