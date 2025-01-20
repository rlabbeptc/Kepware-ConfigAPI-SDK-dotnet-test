using KepwareSync.Configuration;
using KepwareSync.Model;
using KepwareSync.ProjectStorage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public class SyncService : BackgroundService, IDisposable
    {
        private readonly ILogger<SyncService> m_logger;
        private readonly IProjectStorage m_projectStorage;
        private readonly KepServerClient m_kepServerClient;
        private readonly KepSyncOptions m_syncOptions;

        private readonly ConcurrentQueue<ChangeEvent> m_changeQueue = new ConcurrentQueue<ChangeEvent>();

        private IDisposable? m_storageChangeEvents = null;
        private bool m_bIsDisposed = false;
        private long? m_lastProjectId = null;

        public SyncService(KepServerClient kepServerClient, IProjectStorage projectStorage, KepSyncOptions syncOptions, ILogger<SyncService> logger)
        {
            m_logger = logger;
            m_syncOptions = syncOptions;
            m_kepServerClient = kepServerClient;
            m_projectStorage = projectStorage;
        }

        private Task InititalizeAsync()
        {
            m_storageChangeEvents = m_projectStorage.ObserveChanges()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(changeEvent => NotifyChange(new ChangeEvent { Source = ChangeSource.LocalFile, Reason = "Files " + changeEvent.Type.ToString() }));

            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await InititalizeAsync();

            if (m_syncOptions.SyncDirection == SyncDirection.DiskToKepware)
            {
                NotifyChange(new ChangeEvent { Source = ChangeSource.LocalFile, Reason = "Initial sync from local file" });
            }
            else
            {
                NotifyChange(new ChangeEvent { Source = ChangeSource.KepServer, Reason = "Initial sync from KepServer" });
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (m_syncOptions.SyncMode == SyncMode.TwoWay ||
                        m_syncOptions.SyncDirection == SyncDirection.KepwareToDisk)
                    {
                        if (m_changeQueue.TryDequeue(out var changeEvent))
                        {
                            while (m_changeQueue.TryPeek(out var nextPending) && nextPending.Source == changeEvent!.Source)
                            {
                                m_changeQueue.TryDequeue(out changeEvent);
                            }
                            await ProcessChangeAsync(changeEvent!);
                        }
                        else
                        {
                            var currentProjectId = await FetchCurrentProjectIdAsync();
                            if (m_lastProjectId != currentProjectId)
                            {
                                await ProcessChangeAsync(new ChangeEvent { Source = ChangeSource.KepServer, Reason = "Project changed" });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogError(ex, "Error while processing change event");
                }
                await Task.Delay(Math.Max(500, m_syncOptions.SyncThrottlingMs), stoppingToken);
            }
        }

        private async Task<long> FetchCurrentProjectIdAsync()
        {
            var project = await m_kepServerClient.LoadProject(false);

            return project?.ProjectId ?? -1;
        }

        public void NotifyChange(ChangeEvent changeEvent)
        {
            m_logger.LogInformation("Change notification: {ChangeSource} - {Change}", changeEvent.Source, changeEvent.Reason);
            //m_changePipeline.OnNext(changeEvent);
            m_changeQueue.Enqueue(changeEvent);
        }

        private async Task ProcessChangeAsync(ChangeEvent changeEvent)
        {
            m_logger.LogInformation("Processing Sync (most resent event: {ChangeSource} - {Change})", changeEvent.Source, changeEvent.Reason);
            try
            {
                switch (changeEvent.Source)
                {
                    case ChangeSource.KepServer:
                        await SyncFromKepServerAsync();
                        break;

                    case ChangeSource.LocalFile:
                        await SyncFromLocalFileAsync();
                        break;
                }

                m_lastProjectId = await FetchCurrentProjectIdAsync();
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error while processing change event");
            }
        }

        internal async Task SyncFromKepServerAsync()
        {
            m_logger.LogInformation("Synchronizing full project from KepServer to local file...");
            var project = await m_kepServerClient.LoadProject(true);
            await m_projectStorage.ExportProjecAsync(project);
            m_lastProjectId = project.ProjectId;
        }

        internal async Task SyncFromLocalFileAsync()
        {
            try
            {
                m_logger.LogInformation("Synchronizing full project from local file to KepServer...");
                var projectFromDisk = await m_projectStorage.LoadProject(true);
                var projectFromApi = await m_kepServerClient.LoadProject(true);

                if (projectFromDisk.Hash != projectFromApi.Hash)
                {
                    //TODO update project
                    m_logger.LogInformation("[not implemented] Project has changed. Updating project...");
                }
                int inserts = 0, updates = 0, deletes = 0;

                var channelCompare = await m_kepServerClient.CompareAndApply<ChannelCollection, Channel>(projectFromDisk.Channels, projectFromApi.Channels);

                updates += channelCompare.ChangedItems.Count;
                inserts += channelCompare.ItemsOnlyInLeft.Count;
                deletes += channelCompare.ItemsOnlyInRight.Count;

                foreach (var channel in channelCompare.UnchangedItems.Concat(channelCompare.ChangedItems))
                {
                    var deviceCompare = await m_kepServerClient.CompareAndApply<DeviceCollection, Device>(channel.Left!.Devices, channel.Right!.Devices, channel.Right);

                    updates += deviceCompare.ChangedItems.Count;
                    inserts += deviceCompare.ItemsOnlyInLeft.Count;
                    deletes += deviceCompare.ItemsOnlyInRight.Count;

                    foreach (var device in deviceCompare.UnchangedItems.Concat(deviceCompare.ChangedItems))
                    {
                        var tagCompare = await m_kepServerClient.CompareAndApply<DeviceTagCollection, Tag>(device.Left!.Tags, device.Right!.Tags, device.Right);

                        updates += tagCompare.ChangedItems.Count;
                        inserts += tagCompare.ItemsOnlyInLeft.Count;
                        deletes += tagCompare.ItemsOnlyInRight.Count;

                        var tagGroupCompare = await m_kepServerClient.CompareAndApply<DeviceTagGroupCollection, DeviceTagGroup>(device.Left!.TagGroups, device.Right!.TagGroups, device.Right);

                        updates += tagGroupCompare.ChangedItems.Count;
                        inserts += tagGroupCompare.ItemsOnlyInLeft.Count;
                        deletes += tagGroupCompare.ItemsOnlyInRight.Count;


                        foreach (var tagGroup in tagGroupCompare.UnchangedItems.Concat(tagGroupCompare.ChangedItems))
                        {
                            if (tagGroup.Left?.TagGroups != null)
                            {
                                var result = await RecusivlyCompareTagGroup(m_kepServerClient, tagGroup.Left!.TagGroups, tagGroup.Right!.TagGroups, tagGroup.Right);
                                updates += result.updates;
                                inserts += result.inserts;
                                deletes += result.deletes;
                            }
                        }
                    }
                }

                if (updates > 0 || deletes > 0 || inserts > 0)
                {
                    m_logger.LogInformation("Completed synchronisation from disk to kepserver: {NumUpdates} updates, {NumInserts} inserts, {NumDeletes} deletes", updates, inserts, deletes);
                    NotifyChange(new ChangeEvent { Source = ChangeSource.KepServer, Reason = "Sync from kepserver after filesync" });
                }
                else
                {
                    m_logger.LogInformation("Completed synchronisation from disk to kepserver: no changes made");
                }
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error while syncing local project file to KepServer");
            }
        }

        private static async Task<(int inserts, int updates, int deletes)> RecusivlyCompareTagGroup(KepServerClient kepServerClient, DeviceTagGroupCollection left, DeviceTagGroupCollection? right, NamedEntity owner)
        {
            (int inserts, int updates, int deletes) ret = (0, 0, 0);

            var tagGroupCompare = await kepServerClient.CompareAndApply<DeviceTagGroupCollection, DeviceTagGroup>(left, right, owner);

            ret.inserts = tagGroupCompare.ItemsOnlyInLeft.Count;
            ret.updates = tagGroupCompare.ChangedItems.Count;
            ret.deletes = tagGroupCompare.ItemsOnlyInRight.Count;

            foreach (var tagGroup in tagGroupCompare.UnchangedItems.Concat(tagGroupCompare.ChangedItems))
            {
                var tagGroupTagCompare = await kepServerClient.CompareAndApply<DeviceTagGroupTagCollection, Tag>(tagGroup.Left!.Tags, tagGroup.Right!.Tags, tagGroup.Right);

                if (tagGroup.Left!.TagGroups != null)
                {
                    var result = await RecusivlyCompareTagGroup(kepServerClient, tagGroup.Left!.TagGroups, tagGroup.Right!.TagGroups, tagGroup.Right);
                    ret.updates += result.updates;
                    ret.deletes += result.deletes;
                    ret.inserts += result.inserts;
                }
            }

            return ret;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_bIsDisposed)
            {
                if (disposing)
                {
                    m_storageChangeEvents?.Dispose();
                    m_storageChangeEvents = null;
                }

                m_bIsDisposed = true;
            }
        }



        public override void Dispose()
        {
            base.Dispose();
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
