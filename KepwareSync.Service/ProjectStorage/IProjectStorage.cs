using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.SyncService.ProjectStorage
{
    public record StorageChangeEvent
    {
        public enum ChangeType
        {
            added,
            removed,
            changed
        }
        public ChangeType Type { get; }
        public StorageChangeEvent(ChangeType type)
        {
            Type = type;
        }
    }

    public interface IProjectStorage
    {
        public Task<Project> LoadProject(bool blnLoadFullProject = true, CancellationToken cancellationToken = default);
        public Task ExportProjecAsync(Project project, CancellationToken cancellationToken = default);

        public IObservable<StorageChangeEvent> ObserveChanges();
    }
}
