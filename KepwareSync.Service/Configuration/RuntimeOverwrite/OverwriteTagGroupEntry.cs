using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Kepware.Api.Model;
using static Kepware.Api.Model.Docs;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents a tag group containing a list of tag names.
    /// </summary>
    public class OverwriteTagGroupEntry //: IYamlConvertible
    {
        /// <summary>
        /// Gets or sets the name of the tag group.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of tag names in the group.
        /// </summary>
        public List<OverwriteTagEntry> Tags { get; set; } = new List<OverwriteTagEntry>();

        /// <summary>
        /// Gets or sets the list of tag group entries for the device.
        /// </summary>
        public List<OverwriteTagGroupEntry> TagGroups { get; set; } = new List<OverwriteTagGroupEntry>();

        /// <inheritdoc/>
        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            parser.Consume<MappingStart>();
            Name = parser.Consume<Scalar>().Value;
            while (!parser.Accept<MappingEnd>(out _))
            {
                string propertyName = parser.Consume<Scalar>().Value;
                if (propertyName.Equals(nameof(Tags), StringComparison.Ordinal))
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

        /// <inheritdoc/>
        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            throw new NotImplementedException();
        }

        internal bool Apply(DeviceTagGroup group)
        {
            bool blnRet = false;
            var tagMap = group.Tags?.ToDictionary(t => t.Name, StringComparer.InvariantCultureIgnoreCase) ?? [];
            foreach (var tagOverwrite in this.Tags)
            {
                if (tagMap.TryGetValue(tagOverwrite.Name, out var tag))
                {
                    blnRet |= tagOverwrite.Apply(tag);
                }
            }

            var tagGroupMap = group.TagGroups?.ToDictionary(tg => tg.Name, StringComparer.InvariantCultureIgnoreCase) ?? [];
            foreach (var groupOverwrite in this.TagGroups)
            {
                if (tagGroupMap.TryGetValue(groupOverwrite.Name, out var childGroup))
                {
                    blnRet |= groupOverwrite.Apply(childGroup);
                }
            }

            return blnRet;
        }
    }

}
