using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Collections;

namespace KepwareSync.Model
{
    [JsonSerializable(typeof(Project))]
    [JsonSerializable(typeof(List<Channel>))]
    [JsonSerializable(typeof(List<Device>))]
    [JsonSerializable(typeof(List<Tag>))]
    [JsonSerializable(typeof(List<DeviceTagGroup>))]
    [JsonSerializable(typeof(List<DefaultEntity>))]
    public partial class KepJsonContext : JsonSerializerContext
    {
        public static JsonTypeInfo<T> GetJsonTypeInfo<T>()
          where T : BaseEntity
        {
            if (typeof(T) == typeof(Channel))
            {
                return (JsonTypeInfo<T>)(object)Default.Channel;
            }
            else if (typeof(T) == typeof(Project))
            {
                return (JsonTypeInfo<T>)(object)Default.Project;
            }
            else if (typeof(T) == typeof(Device))
            {
                return (JsonTypeInfo<T>)(object)Default.Device;
            }
            else if (typeof(T) == typeof(Tag))
            {
                return (JsonTypeInfo<T>)(object)Default.Tag;
            }
            else if (typeof(T) == typeof(DeviceTagGroup))
            {
                return (JsonTypeInfo<T>)(object)Default.DeviceTagGroup;
            }
            else if (typeof(T) == typeof(DefaultEntity))
            {
                return (JsonTypeInfo<T>)(object)Default.DefaultEntity;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static JsonTypeInfo<List<T>> GetJsonListTypeInfo<T>()
            where T : BaseEntity
        {
            if(typeof(T) == typeof(Channel))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListChannel;
            }
            else if (typeof(T) == typeof(Device))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDevice;
            }
            else if (typeof(T) == typeof(DeviceTagGroup))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDeviceTagGroup;
            }
            else if (typeof(T) == typeof(Tag))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListTag;
            }
            else if (typeof(T) == typeof(DefaultEntity))
            {
                return (JsonTypeInfo<List<T>>)(object)Default.ListDefaultEntity;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
