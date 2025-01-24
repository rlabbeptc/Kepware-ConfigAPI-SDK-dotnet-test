using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.ProjectStorage
{
    internal class YamlProjectStorage : IProjectStorage
    {
        public Task ExportProjecAsync(Project project, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> LoadAsJson(CancellationToken cancellationToken = default)
        {
            return Task.FromResult("{\"project\":\"example\"}"); // Placeholder
        }

        public Task<Project> LoadProject(bool blnLoadFullProject = true, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IObservable<StorageChangeEvent> ObserveChanges()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveFromJson(string projectJson, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
