using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public enum ChangeSource { KepServer, Git, LocalFile }

    public record ChangeEvent
    {
        public ChangeSource Source { get; init; }
        public string? Content { get; init; } // Optional: Content for certain changes
    }
}
