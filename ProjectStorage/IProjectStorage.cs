using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.ProjectStorage
{
    public interface IProjectStorage
    {
        public Task<string> LoadAsJson();
        public Task<bool> SaveFromJson(string projectJson);
    }
}
