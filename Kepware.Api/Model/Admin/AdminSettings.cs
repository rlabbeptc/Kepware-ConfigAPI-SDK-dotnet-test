using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Kepware.Api.Model.Admin
{
    /// <summary>
    /// Represents the administrative settings of the Kepware server.
    /// </summary>
    [Endpoint("/config/v1/admin")]
    public partial class AdminSettings : DefaultEntity
    {
        public AdminSettings()
        {
            LicenseServer = new(this);
        }

        #region LicenseServer
        /// <summary>
        /// Configuration settings for the license server.
        /// Only available when the Kepware server supports licensing via license server like the TKE.
        /// </summary>
        public AdminLicenseServerSettings LicenseServer { get; }
        #endregion

        #region Event Log

        /// <summary>
        /// The TCP/IP port used for event log communication.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? EventLogConnectionPort
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.ConnectionPort);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.ConnectionPort, value);
        }

        /// <summary>
        /// Persistence mode for the event log (Memory, Single File, or Extended Datastore).
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? EventLogPersistence
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.Persistence);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.Persistence, value);
        }

        /// <summary>
        /// Maximum number of records retained in the event log before deletion of oldest records.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? EventLogMaxRecords
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MaxRecords);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.MaxRecords, value);
        }

        /// <summary>
        /// Directory path where event log files are stored.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? EventLogLogFilePath
        {
            get => GetDynamicProperty<string>(Properties.AdminSettings.EventLog.LogFilePath);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.LogFilePath, value);
        }

        /// <summary>
        /// Maximum size in KB of a single event log file.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? EventLogMaxSingleFileSizeKb
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MaxSingleFileSizeKb);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.MaxSingleFileSizeKb, value);
        }

        /// <summary>
        /// Minimum number of days before event log files are deleted.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? EventLogMinDaysToPreserve
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MinDaysToPreserve);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.MinDaysToPreserve, value);
        }

        /// <summary>
        /// Set to true to print event log messages to the console window/stdout.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? EventLogLogToConsole
        {
            get => GetDynamicProperty<bool>(Properties.AdminSettings.EventLog.LogToConsole);
            set => SetDynamicProperty(Properties.AdminSettings.EventLog.LogToConsole, value);
        }

        #endregion

        #region OPC Diagnostics

        /// <summary>
        /// The persistence mode for OPC Diagnostics log data.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? OpcDiagnosticsPersistence
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.Persistence);
            set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.Persistence, value);
        }

        /// <summary>
        /// Maximum number of OPC Diagnostics log records before removal of oldest records.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? OpcDiagnosticsMaxRecords
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MaxRecords);
            set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MaxRecords, value);
        }

        /// <summary>
        /// Directory path where OPC Diagnostics log files are stored.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? OpcDiagnosticsLogFilePath
        {
            get => GetDynamicProperty<string>(Properties.AdminSettings.OpcDiagnostics.LogFilePath);
            set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.LogFilePath, value);
        }

        /// <summary>
        /// Maximum size in KB of a single OPC Diagnostics log file.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? OpcDiagnosticsMaxSingleFileSizeKb
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MaxSingleFileSizeKb);
            set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MaxSingleFileSizeKb, value);
        }

        /// <summary>
        /// Minimum number of days before OPC Diagnostics log files are deleted.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? OpcDiagnosticsMinDaysToPreserve
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MinDaysToPreserve);
            set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MinDaysToPreserve, value);
        }

        #endregion

        #region Comm Diagnostics

        /// <summary>
        /// The persistence mode for Communication Diagnostics log data.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? CommDiagnosticsPersistence
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.CommDiagnostics.Persistence);
            set => SetDynamicProperty(Properties.AdminSettings.CommDiagnostics.Persistence, value);
        }

        /// <summary>
        /// Maximum number of Communication Diagnostics log records before removal of oldest records.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? CommDiagnosticsMaxRecords
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.CommDiagnostics.MaxRecords);
            set => SetDynamicProperty(Properties.AdminSettings.CommDiagnostics.MaxRecords, value);
        }

        /// <summary>
        /// Directory path where Communication Diagnostics log files are stored.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public string? CommDiagnosticsLogFilePath
        {
            get => GetDynamicProperty<string>(Properties.AdminSettings.CommDiagnostics.LogFilePath);
            set => SetDynamicProperty(Properties.AdminSettings.CommDiagnostics.LogFilePath, value);
        }

        /// <summary>
        /// Maximum size in KB of a single Communication Diagnostics log file.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? CommDiagnosticsMaxSingleFileSizeKb
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.CommDiagnostics.MaxSingleFileSizeKb);
            set => SetDynamicProperty(Properties.AdminSettings.CommDiagnostics.MaxSingleFileSizeKb, value);
        }

        /// <summary>
        /// Minimum number of days before Communication Diagnostics log files are deleted.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public int? CommDiagnosticsMinDaysToPreserve
        {
            get => GetDynamicProperty<int>(Properties.AdminSettings.CommDiagnostics.MinDaysToPreserve);
            set => SetDynamicProperty(Properties.AdminSettings.CommDiagnostics.MinDaysToPreserve, value);
        }

        #endregion
    }
}