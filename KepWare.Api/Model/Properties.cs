using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    /// <summary>
    /// Contains constants and static properties related to Kepware API properties.
    /// </summary>
    public static class Properties
    {
        /// <summary>
        /// The device driver property key.
        /// </summary>
        public const string DeviceDriver = "servermain.MULTIPLE_TYPES_DEVICE_DRIVER";
        /// <summary>
        /// The description property key.
        /// </summary>
        public const string Description = "common.ALLTYPES_DESCRIPTION";
        /// <summary>
        /// The name property key.
        /// </summary>
        public const string Name = "common.ALLTYPES_NAME";
        /// <summary>
        /// The project ID property key.
        /// </summary>
        public const string ProjectId = "PROJECT_ID";
        /// <summary>
        /// The channel unique ID property key.
        /// </summary>
        public const string ChannelUid = "servermain.CHANNEL_UNIQUE_ID";

        /// <summary>
        /// Contains constants related to tag properties.
        /// </summary>
        public static class Tag
        {
            /// <summary>
            /// The address property key.
            /// </summary>
            public const string Address = "servermain.TAG_ADDRESS";
            /// <summary>
            /// The data type property key.
            /// </summary>
            public const string DataType = "servermain.TAG_DATA_TYPE";
            /// <summary>
            /// The read/write access property key.
            /// </summary>
            public const string ReadWriteAccess = "servermain.TAG_READ_WRITE_ACCESS";
            /// <summary>
            /// The scan rate in milliseconds property key.
            /// </summary>
            public const string ScanRateMilliseconds = "servermain.TAG_SCAN_RATE_MILLISECONDS";
            /// <summary>
            /// The scaling type property key.
            /// </summary>
            public const string ScalingType = "servermain.TAG_SCALING_TYPE";
            /// <summary>
            /// The scaling raw low property key.
            /// </summary>
            public const string ScalingRawLow = "servermain.TAG_SCALING_RAW_LOW";
            /// <summary>
            /// The scaling raw high property key.
            /// </summary>
            public const string ScalingRawHigh = "servermain.TAG_SCALING_RAW_HIGH";
            /// <summary>
            /// The scaling scaled low property key.
            /// </summary>
            public const string ScalingScaledLow = "servermain.TAG_SCALING_SCALED_LOW";
            /// <summary>
            /// The scaling scaled high property key.
            /// </summary>
            public const string ScalingScaledHigh = "servermain.TAG_SCALING_SCALED_HIGH";
            /// <summary>
            /// The scaling scaled data type property key.
            /// </summary>
            public const string ScalingScaledDataType = "servermain.TAG_SCALING_SCALED_DATA_TYPE";
            /// <summary>
            /// The scaling clamp low property key.
            /// </summary>
            public const string ScalingClampLow = "servermain.TAG_SCALING_CLAMP_LOW";
            /// <summary>
            /// The scaling clamp high property key.
            /// </summary>
            public const string ScalingClampHigh = "servermain.TAG_SCALING_CLAMP_HIGH";
            /// <summary>
            /// The scaling units property key.
            /// </summary>
            public const string ScalingUnits = "servermain.TAG_SCALING_UNITS";
            /// <summary>
            /// The scaling negate value property key.
            /// </summary>
            public const string ScalingNegateValue = "servermain.TAG_SCALING_NEGATE_VALUE";

            /// <summary>
            /// A set of properties to ignore when scaling is disabled.
            /// </summary>
            public static readonly FrozenSet<string> IgnoreWhenScalingDisalbedHashSet = new HashSet<string> {
                        ScalingScaledDataType,
                        ScalingScaledHigh,
                        ScalingScaledLow,
                        ScalingRawHigh,
                        ScalingRawLow,
                        ScalingClampHigh,
                        ScalingClampLow,
                        ScalingNegateValue,
                        ScalingUnits,
                    }.ToFrozenSet();
        }

        /// <summary>
        /// Contains constants related to non-updatable properties.
        /// </summary>
        public static class NonUpdatable
        {
            /// <summary>
            /// The channel unique ID property key.
            /// </summary>
            public const string ChannelUniqueId = "servermain.CHANNEL_UNIQUE_ID";
            /// <summary>
            /// The device unique ID property key.
            /// </summary>
            public const string DeviceUniqueId = "servermain.DEVICE_UNIQUE_ID";

            /// <summary>
            /// A set of non-updatable properties.
            /// </summary>
            public static readonly FrozenSet<string> AsHashSet = new HashSet<string>()
                {
                    ChannelUniqueId,
                    DeviceUniqueId,
                }.ToFrozenSet();
        }

        /// <summary>
        /// Contains constants related to non-serialized properties.
        /// </summary>
        public static class NonSerialized
        {
            /// <summary>
            /// The channel assignment property key.
            /// </summary>
            public const string ChannelAssignment = "servermain.DEVICE_CHANNEL_ASSIGNMENT";
            /// <summary>
            /// The total tag count in a tag group property key.
            /// </summary>
            public const string TagGrpTotalTagCount = "servermain.TAGGROUP_TOTAL_TAG_COUNT";
            /// <summary>
            /// The local tag count in a tag group property key.
            /// </summary>
            public const string TagGrpTagCount = "servermain.TAGGROUP_LOCAL_TAG_COUNT";
            /// <summary>
            /// The static tag count in a channel property key.
            /// </summary>
            public const string ChannelTagCount = "servermain.CHANNEL_STATIC_TAG_COUNT";
            /// <summary>
            /// The autogenerated tag group property key.
            /// </summary>
            public const string TagGroupAutogenerated = "servermain.TAGGROUP_AUTOGENERATED";
            /// <summary>
            /// The autogenerated tag property key.
            /// </summary>
            public const string TagAutogenerated = "servermain.TAG_AUTOGENERATED";
            /// <summary>
            /// The static tag count in a device property key.
            /// </summary>
            public const string DeviceStaticTagCount = "servermain.DEVICE_STATIC_TAG_COUNT";

            /// <summary>
            /// A set of non-serialized properties.
            /// </summary>
            public static readonly FrozenSet<string> AsHashSet = new HashSet<string>()
                    {
                        ChannelAssignment,
                        TagGrpTotalTagCount,
                        TagGrpTagCount,
                        ChannelTagCount,
                        TagGroupAutogenerated,
                        TagAutogenerated ,
                        DeviceStaticTagCount,
                    }.ToFrozenSet();
        }
    }
}
