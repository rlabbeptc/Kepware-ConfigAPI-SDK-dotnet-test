using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Kepware.Api.Model;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents a device configuration with overwrite entries, tag overwrite entries, and tag groups.
    /// </summary>
    public class OverwriteDeviceEntry : OverwriteBaseEntry
    {
        /// <summary>   
        /// Gets or sets the list of tag overwrite entries for the device.
        /// </summary>
        [YamlMember(Alias = "Tags")]
        public List<OverwriteTagEntry> Tags { get; set; } = new List<OverwriteTagEntry>();

        /// <summary>
        /// Gets or sets the list of tag group entries for the device.
        /// </summary>
        [YamlMember(Alias = "TagGroups")]
        public List<OverwriteTagGroupEntry> TagGroups { get; set; } = new List<OverwriteTagGroupEntry>();

        /// <inheritdoc/>
        public override void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();
            Name = parser.Consume<Scalar>().Value;
            while (!parser.Accept<MappingEnd>(out _))
            {
                string propertyName = parser.Consume<Scalar>().Value;
                if (propertyName.Equals(nameof(Overwrite), StringComparison.Ordinal))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
                        var entry = (OverwriteProperty)nestedObjectDeserializer(typeof(OverwriteProperty))!;
                        Overwrite.Add(entry);
                    }
                    parser.Consume<SequenceEnd>();
                }
                else if (propertyName.Equals(nameof(Tags), StringComparison.Ordinal))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
                        var tag = (OverwriteTagEntry)nestedObjectDeserializer(typeof(OverwriteTagEntry))!;
                        Tags.Add(tag);
                    }
                    parser.Consume<SequenceEnd>();
                }
                else if (propertyName.Equals(nameof(TagGroups), StringComparison.Ordinal))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.Accept<SequenceEnd>(out _))
                    {
                        var group = (OverwriteTagGroupEntry)nestedObjectDeserializer(typeof(OverwriteTagGroupEntry))!;
                        TagGroups.Add(group);
                    }
                    parser.Consume<SequenceEnd>();
                }
                else
                {
                    // Skip unknown properties.
                    nestedObjectDeserializer(typeof(object));
                }
            }
            parser.Consume<MappingEnd>();
        }

        public override bool Apply(BaseEntity entity)
        {
            if (entity is Device device)
            {
                bool blnRet = base.Apply(entity);
                var tagMap = device.Tags?.ToDictionary(t => t.Name, StringComparer.InvariantCultureIgnoreCase) ?? [];
                foreach (var tagOverwrite in this.Tags)
                {
                    if (tagMap.TryGetValue(tagOverwrite.Name, out var tag))
                    {
                        blnRet |= tagOverwrite.Apply(tag);
                    }
                }
                var tagGroupMap = device.TagGroups?.ToDictionary(tg => tg.Name, StringComparer.InvariantCultureIgnoreCase) ?? [];
                foreach (var groupOverwrite in this.TagGroups)
                {
                    if (tagGroupMap.TryGetValue(groupOverwrite.Name, out var group))
                    {
                        blnRet |= groupOverwrite.Apply(group);
                    }
                }

                return blnRet;
            }
            else
            {
                throw new InvalidOperationException($"Cannot apply {nameof(OverwriteDeviceEntry)} to entity of type {entity.GetType().Name}.");
            }
        }
    }
}