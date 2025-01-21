using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public class KepStorageOptions
    {
        public string? Directory { get; set; }
        public bool? PersistDefaultValue { get; set; }
    }
}
