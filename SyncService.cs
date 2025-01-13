using KepwareSync.ProjectStorage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace KepwareSync
{
    public class SyncService
    {
        private readonly Subject<ChangeEvent> m_changePipeline;
        private readonly ILogger<SyncService> m_logger;
        private readonly IProjectStorage m_projectStorage;
        private readonly KepServerClient m_kepServerClient;
        private readonly GitClient m_gitClient;

        public SyncService(ILogger<SyncService> logger, KepServerClient kepServerClient, GitClient gitClient, IProjectStorage projectStorage)
        {
            m_changePipeline = new Subject<ChangeEvent>();
            m_logger = logger;
            m_kepServerClient = kepServerClient;
            m_gitClient = gitClient;
            m_projectStorage = projectStorage;
            SetupPipeline();
        }

        private void SetupPipeline()
        {
            m_changePipeline
                .Throttle(TimeSpan.FromMilliseconds(500)) // Debounce for burst changes
                .Subscribe(async change => await ProcessChangeAsync(change));
        }

        public void NotifyChange(ChangeEvent changeEvent)
        {
            m_logger.LogInformation($"Change detected: {changeEvent.Source} - {changeEvent}");
            m_changePipeline.OnNext(changeEvent);
        }

        private async Task ProcessChangeAsync(ChangeEvent change)
        {
            m_logger.LogInformation($"Processing Change: {change.Source} - {change}");

            switch (change.Source)
            {
                case ChangeSource.KepServer:
                    await SyncFromKepServerAsync();
                    break;

                case ChangeSource.Git:
                    await SyncFromGitAsync();
                    break;

                case ChangeSource.LocalFile:
                    await SyncFromLocalFileAsync(change);
                    break;
            }
        }

        private async Task SyncFromKepServerAsync()
        {
            m_logger.LogInformation("Fetching full project from KepServer...");
            var projectJson = await m_kepServerClient.GetFullProjectAsync();

            // Save project locally
            if (await m_projectStorage.SaveFromJson(projectJson))
            {
                m_logger.LogInformation($"Saved KepServer project");

                // Commit and push to GIT
                await m_gitClient.CommitAndPushAsync("Synced KepServer project to GIT");
            }
            else
            {
                m_logger.LogError($"Failed to save KepServer project");
            }
        }

        private async Task SyncFromGitAsync()
        {
            m_logger.LogInformation("Fetching full project from GIT...");
            var projectJson = await m_gitClient.GetFullProjectAsync();

            // Update KepServer
            await m_kepServerClient.UpdateFullProjectAsync(projectJson);
            m_logger.LogInformation("Updated KepServer with project from GIT");
        }

        private async Task SyncFromLocalFileAsync(ChangeEvent change)
        {
            m_logger.LogInformation("Reading local project file...");
            var projectJson = await m_projectStorage.LoadAsJson();

            // Update KepServer
            await m_kepServerClient.UpdateFullProjectAsync(projectJson);
            m_logger.LogInformation("Updated KepServer with local project file");

            // Commit and push to GIT
            await m_gitClient.CommitAndPushAsync("Synced local project file to GIT");
        }
    }
}
