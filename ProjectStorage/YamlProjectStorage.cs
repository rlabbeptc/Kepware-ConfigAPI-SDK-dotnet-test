using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.ProjectStorage
{
    internal class YamlProjectStorage : IProjectStorage
    {
        public Task<string> LoadAsJson()
        {
            return Task.FromResult("{\"project\":\"example\"}"); // Placeholder
        }

        public Task<bool> SaveFromJson(string projectJson)
        {
            return Task.FromResult(true);
        }
    }
}
