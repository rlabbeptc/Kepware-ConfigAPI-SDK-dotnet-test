using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public class KepApiOptions
    {
        public KepApiHost Primary { get; set; } = KepApiHost.Empty;
        public List<KepApiHost> Secondary { get; set; } = [];
        public int TimeoutInSeconds { get; set; } = 60;
        public bool DisableCertificateValidation { get; set; } = false;
    }

    public class KepApiHost
    {
        public static readonly KepApiHost Empty = new();

        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
    }
}
