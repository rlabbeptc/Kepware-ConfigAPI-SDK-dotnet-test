using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Events;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using Kepware.Api.Util;

namespace Kepware.SyncService.Configuration.RuntimeOverwrite
{
    /// <summary>
    /// Represents a single key/value overwrite entry.
    /// </summary>
    public partial class OverwriteProperty : IYamlConvertible
    {
        /// <summary>
        /// Gets or sets the key of the overwrite entry.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the overwrite entry.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <inheritdoc/>
        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer)
        {
            // Consume a mapping with one key/value pair.
            parser.Consume<MappingStart>();
            Key = parser.Consume<Scalar>().Value;
            Value = EnvVariableResolver.ResolveEnvironmentVariables(parser.Consume<Scalar>().Value);
            parser.Consume<MappingEnd>();
        }

        /// <inheritdoc/>
        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));
            emitter.Emit(new Scalar(Key));
            emitter.Emit(new Scalar(Value));
            emitter.Emit(new MappingEnd());
        }

        internal bool GetValueAsBool()
        {
            if (Boolean.TryParse(Value, out var boolValue))
                return boolValue;
            else
                return Value == "1" || Value == bool.TrueString;
        }

        internal int GetValueAsInt()
        {
            if (Int32.TryParse(Value, out var intNumber))
                return intNumber;
            else
                throw new InvalidCastException($"Could not convert {Value} to a number for {Key}");
        }
    }
}