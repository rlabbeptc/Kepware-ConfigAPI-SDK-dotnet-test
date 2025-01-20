using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public enum ChangeSource { KepServer, LocalFile }

    public record ChangeEvent
    {
        public ChangeSource Source { get; init; }
        public string? Reason { get; init; } // Optional: Content for certain changes
    }
}
