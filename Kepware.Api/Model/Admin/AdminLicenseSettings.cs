using Kepware.Api.Model.Kepware.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kepware.Api.Model.Admin
{
    [Endpoint("/config/v1/admin")]
    public class AdminLicenseServerSettings : AdminSettings
    {
        /// <summary>
        /// Host name or IP address for the license server (character limit is 63 characters).
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? LicensingServerName
        {
            get => GetDynamicProperty<string>(Properties.AdminSettings.Licensing.ServerName);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.ServerName, value);
        }

        /// <summary>
        /// Enables or disables the connection to the licensing server.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? LicensingServerEnable
        {
            get => GetDynamicProperty<bool>(Properties.AdminSettings.Licensing.ServerEnable);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.ServerEnable, value);
        }

        /// <summary>
        /// The port number used to connect to license server for non-TLS connections
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? LicensingServerPort
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.Licensing.ServerPort);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.ServerPort, value);
        }


        /// <summary>
        /// The port number used for TLS-secured connections to the licensing server.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? LicensingServerSslPort
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.Licensing.ServerSslPort);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.ServerSslPort, value);
        }

        /// <summary>
        /// Allows insecure (non-TLS) communication with the licensing server.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? LicensingAllowInsecureComms
        {
            get => GetDynamicProperty<bool>(Properties.AdminSettings.Licensing.AllowInsecureComms);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.AllowInsecureComms, value);
        }

        /// <summary>
        /// Allows the use of self-signed certificates for licensing server communication.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? LicensingAllowSelfSignedCerts
        {
            get => GetDynamicProperty<bool>(Properties.AdminSettings.Licensing.AllowSelfSignedCerts);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.AllowSelfSignedCerts, value);
        }

        /// <summary>
        /// Custom alias used for requesting a license from the licensing server (character limit: 63).
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? LicensingClientAlias
        {
            get => GetDynamicProperty<string>(Properties.AdminSettings.Licensing.ClientAlias);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.ClientAlias, value);
        }

        /// <summary>
        /// Time interval (in minutes) between automatic license state checks.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? LicensingRecheckIntervalMinutes
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.Licensing.RecheckIntervalMinutes);
            set => SetDynamicProperty(Properties.AdminSettings.Licensing.RecheckIntervalMinutes, value);
        }

    }
}
