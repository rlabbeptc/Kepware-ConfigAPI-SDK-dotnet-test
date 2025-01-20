using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace KepwareSync.Model
{
    [YamlStaticContext]
    [YamlSerializable(typeof(Channel))]
    [YamlSerializable(typeof(Device))]
    [YamlSerializable(typeof(DeviceTagGroup))]
    [YamlSerializable(typeof(DefaultEntity))]
    public partial class KepYamlContext : StaticContext
    {
       
    }
}
