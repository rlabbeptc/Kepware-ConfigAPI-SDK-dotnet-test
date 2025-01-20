using KepwareSync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync.ProjectStorage
{
    internal class YamlProjectStorage : IProjectStorage
    {
        public Task ExportProjecAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public Task<string> LoadAsJson()
        {
            return Task.FromResult("{\"project\":\"example\"}"); // Placeholder
        }

        public Task<Project> LoadProject(bool blnLoadFullProject = true)
        {
            throw new NotImplementedException();
        }

        public IObservable<StorageChangeEvent> ObserveChanges()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFromJson(string projectJson)
        {
            return Task.FromResult(true);
        }
    }
}
