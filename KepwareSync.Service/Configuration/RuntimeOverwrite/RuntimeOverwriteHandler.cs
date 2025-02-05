using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    [YamlStaticContext]
    [YamlSerializable(typeof(RuntimeOverwriteConfig))]
    [YamlSerializable(typeof(OverwriteChannelEntry))]
    [YamlSerializable(typeof(OverwriteDeviceEntry))]
    [YamlSerializable(typeof(OverwriteTagEntry))]
    [YamlSerializable(typeof(OverwriteTagGroupEntry))]
    [YamlSerializable(typeof(OverwriteProperty))]
    public partial class RuntimeOverwriteConfigYamlContext : StaticContext
    {
    }

    public partial class RuntimeOverwriteConfig
    {
        public static RuntimeOverwriteConfig LoadFromYaml(string yamlContent)
        {
            return RuntimeOverwriteHandler.LoadYaml(yamlContent);
        }

        public static Task<RuntimeOverwriteConfig> LoadFromYamlFileAsync(string yamlContent, CancellationToken cancellationToken = default)
        {
            return RuntimeOverwriteHandler.ReadYamlFileAsync(yamlContent, cancellationToken);
        }

        /// <summary>
        /// Provides functionality to load and process runtime overwrite configuration from YAML.
        /// </summary>
        private static class RuntimeOverwriteHandler
        {
            private static readonly IDeserializer s_deserializer = new StaticDeserializerBuilder(new RuntimeOverwriteConfigYamlContext())
                    .WithNamingConvention(NullNamingConvention.Instance)
                    .Build();

            public static async Task<RuntimeOverwriteConfig> ReadYamlFileAsync(string yamlFile, CancellationToken cancellationToken = default)
            {
                return LoadFromYaml(await File.ReadAllTextAsync(yamlFile, cancellationToken).ConfigureAwait(false));
            }

            /// <summary>
            /// Loads the runtime overwrite configuration from a YAML string.
            /// </summary>
            /// <param name="yamlContent">The YAML content.</param>
            /// <returns>An instance of <see cref="RuntimeOverwriteConfig"/>.</returns>
            public static RuntimeOverwriteConfig LoadYaml(string yamlContent)
            {
                var config = s_deserializer.Deserialize<RuntimeOverwriteConfig>(yamlContent);
                ProcessEnvironmentVariables(config);
                return config;
            }

            /// <summary>
            /// Processes environment variable placeholders in overwrite entries.
            /// </summary>
            /// <param name="config">The runtime overwrite configuration.</param>
            private static void ProcessEnvironmentVariables(RuntimeOverwriteConfig config)
            {
                foreach (var channel in config.Channels)
                {
                    ResolveEnvVars(channel.Overwrite);
                    foreach (var device in channel.Devices)
                    {
                        ResolveEnvVars(device.Overwrite);
                        // Additional processing for tag overwrite entries can be added here.
                    }
                }
            }

            /// <summary>
            /// Replaces values in overwrite entries if they are in the form ${ENV_VAR}.
            /// </summary>
            /// <param name="entries">The list of overwrite entries.</param>
            private static void ResolveEnvVars(List<OverwriteProperty> entries)
            {
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrEmpty(entry.Value) &&
                        entry.Value.StartsWith("${") && entry.Value.EndsWith("}"))
                    {
                        var envVar = entry.Value[2..^1];
                        var envValue = Environment.GetEnvironmentVariable(envVar);
                        if (envValue != null)
                        {
                            entry.Value = envValue;
                        }
                    }
                }
            }
        }
    }
}
