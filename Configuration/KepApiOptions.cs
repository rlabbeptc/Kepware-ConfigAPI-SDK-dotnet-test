using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.Configuration
{
    public class KepApiOptions
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }

        public int TimeoutInSeconds { get; set; } = 60;
    }
}
