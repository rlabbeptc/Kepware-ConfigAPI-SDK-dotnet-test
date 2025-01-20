using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using KepwareSync.Model;
using KepwareSync.Serializer;
using YamlDotNet.Serialization.NodeTypeResolvers;
using System.Security.AccessControl;
using KepwareSync.Configuration;
using System.Text.Json;

namespace KepwareSync
{
    internal class KepFolderStorage
    {
        private readonly YamlSerializer m_yamlSerializer;
        private readonly CsvTagSerializer m_csvTagSerializer;
        private readonly DataTypeEnumConverterProvider m_dataTypeEnumConverterProvider;
        private readonly ILogger<KepFolderStorage> m_logger;
        private readonly KepStorageOptions m_kepStorageOptions;
        private readonly DirectoryInfo m_baseDirectory;

        public KepFolderStorage(ILogger<KepFolderStorage> logger, KepStorageOptions kepStorageOptions)
        {
            m_logger = logger;
            m_kepStorageOptions = kepStorageOptions;
            m_yamlSerializer = new YamlSerializer();
            m_csvTagSerializer = new CsvTagSerializer();
            m_dataTypeEnumConverterProvider = new DataTypeEnumConverterProvider();
            m_baseDirectory = new DirectoryInfo(m_kepStorageOptions.Directory ?? "ExportedYaml");
            if (!m_baseDirectory.Exists)
                m_baseDirectory.Create();
        }

        public async Task<Project> LoadProject(bool blnLoadDeep = true)
        {
            var projectFile = Path.Combine(m_baseDirectory.FullName, "project.yaml");

            var project = await m_yamlSerializer.LoadFromYaml<Project>(projectFile);
            if (project == null) return new Project();

            if (blnLoadDeep)
            {
                project.Channels = await LoadChannels(blnLoadDeep);
            }

            return project;
        }

        public async Task<ChannelCollection> LoadChannels(bool blnLoadDeep = true)
        {
            var channelDirs = m_baseDirectory.GetDirectories();
            var channels = new ChannelCollection();
            foreach (var channelDir in channelDirs)
            {
                channels.Items.Add(await LoadChannel(channelDir.Name, blnLoadDeep));
            }
            return channels;
        }

        public Task<Channel> LoadChannel(string channelName, bool blnLoadDeep = true)
            => LoadChannel(new DirectoryInfo(Path.Combine(m_baseDirectory.FullName, channelName)), blnLoadDeep);

        public async Task<Channel> LoadChannel(DirectoryInfo channelDir, bool blnLoadDeep = true)
        {
            var channelFile = Path.Combine(channelDir.FullName, "channel.yaml");

            var channel = await m_yamlSerializer.LoadFromYaml<Channel>(channelFile);
            if (channel == null) return new Channel();
            if (blnLoadDeep)
            {
                channel.Devices = await LoadDevices(channelDir, blnLoadDeep);
            }
            return channel;
        }

        private async Task<DeviceCollection> LoadDevices(DirectoryInfo channelDir, bool blnLoadDeep = true)
        {
            var devices = new DeviceCollection();
            var deviceDirs = channelDir.GetDirectories();
            foreach (var deviceDir in deviceDirs)
            {
                devices.Items.Add(await LoadDevice(deviceDir, blnLoadDeep));
            }
            return devices;
        }

        private async Task<Device> LoadDevice(DirectoryInfo deviceDir, bool blnLoadDeep = true)
        {
            var deviceFile = Path.Combine(deviceDir.FullName, "device.yaml");
            var device = await m_yamlSerializer.LoadFromYaml<Device>(deviceFile);
            if (device == null) return new Device();
            if (blnLoadDeep)
            {
                device.Tags = new DeviceTagCollection { Items = await LoadTags(device, deviceDir) };
                device.TagGroups = await LoadTagGroups(device, deviceDir);
            }
            return device;
        }

        private async Task<DeviceTagGroupCollection?> LoadTagGroups(Device device, DirectoryInfo deviceDir)
        {
            var tagGroups = new DeviceTagGroupCollection();
            var tagGroupDirs = deviceDir.GetDirectories();

            foreach (var grpDir in tagGroupDirs)
            {
                var tagGroupFile = Path.Combine(grpDir.FullName, "tagGroup.yaml");
                var tagGroup = await m_yamlSerializer.LoadFromYaml<DeviceTagGroup>(tagGroupFile);
                tagGroup.Tags = new DeviceTagGroupTagCollection { Items = await LoadTags(device, grpDir) };
                tagGroup.TagGroups = await LoadTagGroups(device, grpDir);

                tagGroups.Items.Add(tagGroup);
            }

            return tagGroups;
        }

        private Task<List<Tag>> LoadTags(Device device, DirectoryInfo tagDirectory)
        {
            var tagFile = Path.Combine(tagDirectory.FullName, "tags.csv");
            if (!File.Exists(tagFile))
                return Task.FromResult(new List<Tag>());

            var dataTypeConverter = m_dataTypeEnumConverterProvider.GetDataTypeEnumConverter(device.GetDynamicProperty<string>(Properties.DeviceDriver));
            return m_csvTagSerializer.ImportTagsAsync(tagFile, dataTypeConverter);
        }

        internal async Task ExportProjecAsync(Project project)
        {
            try
            {
                var projectFile = Path.Combine(m_baseDirectory.FullName, "project.yaml");
                await m_yamlSerializer.SaveAsYaml(projectFile, project);
                await ExportChannelsAsync(project.Channels);

#if DEBUG
                project.Cleanup(true);
                await File.WriteAllTextAsync(Path.Combine(m_baseDirectory.FullName, "project.json"), JsonSerializer.Serialize(new JsonProjectRoot { Project = project }, KepJsonContext.Default.JsonProjectRoot));
#endif
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error exporting project");
            }
        }
        public async Task ExportChannelsAsync(ChannelCollection? channels)
        {
            if (channels == null) return;

            try
            {
                var channelDirsToDelete = m_baseDirectory.GetDirectories().Select(dir => dir.Name)
                  .ToHashSet(StringComparer.OrdinalIgnoreCase);

                List<(string baseDir, IEnumerable<string> folderNames)> foldersToDelete = new();
                int exportedChannels = 0, exportedDevices = 0, exportedTags = 0;
                foreach (var channel in channels)
                {
                    var fileSaveName = channel.Name.EscapeDiskEntry();

                    var channelFolder = new DirectoryInfo(Path.Combine(m_baseDirectory.FullName, fileSaveName));
                    if (!channelFolder.Exists)
                        channelFolder.Create();
                    channelDirsToDelete.Remove(fileSaveName);
                    ++exportedChannels;
                    try
                    {
                        // Export Channel
                        var channelFile = Path.Combine(channelFolder.FullName, "channel.yaml");
                        await m_yamlSerializer.SaveAsYaml(channelFile, channel);
                    }
                    catch (Exception ex)
                    {
                        m_logger.LogError(ex, "Error exporting channel '{ChannelName}' to YAML.", channel.Name);
                        continue;
                    }

                    if (channel.Devices != null)
                    {
                        var deviceDirsToDelete = new DirectoryInfo(channelFolder.FullName).GetDirectories().Select(dir => dir.Name)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        var count = await ExportDevices(channel, channelFolder.FullName, deviceDirsToDelete);

                        foldersToDelete.Add((channelFolder.FullName, deviceDirsToDelete));
                    }
                }

                try
                {
                    DeleteDirectories(m_baseDirectory.FullName, channelDirsToDelete);

                    foreach (var (baseDir, folderNames) in foldersToDelete)
                    {
                        DeleteDirectories(baseDir, folderNames);
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogError(ex, "Error deleting directories.");
                }

                // Log information: Exported channels to YAML successfully (including target directory, count of channels, devices, and tags).
                m_logger.LogInformation(
                    "Exported channels to YAML successfully. Target directory: {TargetDirectory}, Channels: {ChannelCount}, Devices: {DeviceCount}, Tags: {TagCount}",
                    m_baseDirectory.FullName,
                    exportedChannels,
                    exportedDevices,
                    exportedTags);

            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Error exporting channels to YAML.");
            }
        }


        private void DeleteDirectories(string baseDir, IEnumerable<string> names)
        {
            // Delete all channel directories that are not in the current project   
            foreach (var dir in names)
            {
                var folderPath = Path.Combine(baseDir, dir);
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    m_logger.LogInformation("Deleted folder: {FolderPath}", folderPath);
                }
            }
        }

        public async Task ExportDevices(Channel channel, DeviceCollection? devices)
        {
            string channelFolder = Path.Combine(m_baseDirectory.FullName, channel.Name.EscapeDiskEntry());

            var deviceDirsToDelete = new DirectoryInfo(channelFolder).GetDirectories().Select(dir => dir.Name)
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);
            await ExportDevices(channel, channelFolder, deviceDirsToDelete);
            DeleteDirectories(channelFolder, deviceDirsToDelete);
        }

        private async Task<(int exportedDevices, int exportedTags)> ExportDevices(Channel channel, string channelFolder, HashSet<string> deviceDirsToDelete)
        {
            if (channel?.Devices == null)
                return (0, 0);

            int exportedDevices = 0, exportedTags = 0;

            foreach (var device in channel.Devices)
            {
                ++exportedDevices;

                string fileSaveName = device.Name.EscapeDiskEntry();

                var deviceFolder = Path.Combine(channelFolder, fileSaveName);
                if (!Directory.Exists(deviceFolder))
                    Directory.CreateDirectory(deviceFolder);

                var dataTypeConverter = m_dataTypeEnumConverterProvider.GetDataTypeEnumConverter(device.GetDynamicProperty<string>(Properties.DeviceDriver));
                deviceDirsToDelete.Remove(fileSaveName);

                try
                {
                    // Export Device
                    var deviceFile = Path.Combine(deviceFolder, "device.yaml");
                    await m_yamlSerializer.SaveAsYaml(deviceFile, device);

                    if (device.Tags != null)
                    {
                        await m_csvTagSerializer.ExportTagsAsync(Path.Combine(deviceFolder, "tags.csv"), device.Tags.Items, dataTypeConverter);
                        exportedTags += device.Tags.Items.Count;
                    }

                    if (device.TagGroups != null)
                    {
                        exportedTags += await ExportTagGroups(deviceFolder, device.TagGroups, dataTypeConverter);
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogError(ex, "Error exporting device '{DeviceName}' to YAML.", device.Name);
                }
            }
            return (exportedDevices, exportedTags);
        }

        private async Task<int> ExportTagGroups(string parentFolder, IEnumerable<DeviceTagGroup> tagGroups, IDataTypeEnumConverter dataTypeConverter)
        {
            int exportedTags = 0;
            foreach (var tagGroup in tagGroups)
            {
                var tagGroupFolder = Path.Combine(parentFolder, tagGroup.Name.EscapeDiskEntry());

                // Export TagGroup
                var tagGroupFile = Path.Combine(tagGroupFolder, "tagGroup.yaml");
                await m_yamlSerializer.SaveAsYaml(tagGroupFile, tagGroup);

                if (tagGroup.Tags != null)
                {
                    await m_csvTagSerializer.ExportTagsAsync(Path.Combine(tagGroupFolder, "tags.csv"), tagGroup.Tags.Items, dataTypeConverter);
                    exportedTags += tagGroup.Tags.Items.Count;
                }

                if (tagGroup.TagGroups != null && tagGroup.TagGroups.Items.Count > 0)
                {
                    exportedTags += await ExportTagGroups(tagGroupFolder, tagGroup.TagGroups.Items, dataTypeConverter);
                }
            }
            return exportedTags;
        }
    }
}
