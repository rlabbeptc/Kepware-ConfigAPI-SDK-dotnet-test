using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.Configuration
{
    public class KepApiOptions
    {
        public KepwareSyncTarget Primary { get; set; } = KepwareSyncTarget.Empty;
        public List<KepwareSyncTarget> Secondary { get; set; } = [];
        public int TimeoutInSeconds { get; set; } = 60;
        public bool DisableCertificateValidation { get; set; } = false;
    }

    public class KepwareSyncTarget
    {
        public static readonly KepwareSyncTarget Empty = new();

        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }

        public string? PasswordFile { get; set; }
        public string? OverwriteConfigFile { get; set; }
    }
}
