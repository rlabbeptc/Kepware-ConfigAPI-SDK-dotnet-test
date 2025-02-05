using Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents the root configuration for runtime overwrite.
    /// </summary>
    public partial class RuntimeOverwriteConfig
    {
        /// <summary>
        /// Gets or sets the list of channels.
        /// </summary>
        [YamlMember(Alias = "Channels")]
        public List<OverwriteChannelEntry> Channels { get; set; } = new List<OverwriteChannelEntry>();

        public bool Apply(Project project)
        {
            bool blnRet = false;
            var channelMap = project.Channels?.ToDictionary(c => c.Name) ?? [];
            foreach (var channel in Channels)
            {
                if (channelMap.TryGetValue(channel.Name, out var projectChannel))
                {
                    blnRet |= channel.Apply(projectChannel);
                }
            }
            return blnRet;
        }
    }
}
