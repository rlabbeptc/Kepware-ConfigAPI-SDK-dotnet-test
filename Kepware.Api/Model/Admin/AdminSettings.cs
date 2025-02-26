using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    using System;

    namespace Kepware.Api.Model
    {
        /// <summary>
        /// Represents the administrative settings of the Kepware server.
        /// </summary>
        [Endpoint("/config/v1/admin")]
        public class AdminSettings : DefaultEntity
        {
            #region Event Log

            /// <summary>
            /// The TCP/IP port used for event log communication.
            /// </summary>
            public int? EventLogConnectionPort
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.ConnectionPort);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.ConnectionPort, value);
            }

            /// <summary>
            /// Persistence mode for the event log (Memory, Single File, or Extended Datastore).
            /// </summary>
            public string? EventLogPersistence
            {
                get => GetDynamicProperty<string>(Properties.AdminSettings.EventLog.Persistence);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.Persistence, value);
            }

            /// <summary>
            /// Maximum number of records retained in the event log before deletion of oldest records.
            /// </summary>
            public int? EventLogMaxRecords
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MaxRecords);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.MaxRecords, value);
            }

            /// <summary>
            /// Directory path where event log files are stored.
            /// </summary>
            public string? EventLogLogFilePath
            {
                get => GetDynamicProperty<string>(Properties.AdminSettings.EventLog.LogFilePath);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.LogFilePath, value);
            }

            /// <summary>
            /// Maximum size in KB of a single event log file.
            /// </summary>
            public int? EventLogMaxSingleFileSizeKb
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MaxSingleFileSizeKb);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.MaxSingleFileSizeKb, value);
            }

            /// <summary>
            /// Minimum number of days before event log files are deleted.
            /// </summary>
            public int? EventLogMinDaysToPreserve
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.EventLog.MinDaysToPreserve);
                set => SetDynamicProperty(Properties.AdminSettings.EventLog.MinDaysToPreserve, value);
            }

            #endregion

            #region OPC Diagnostics

            /// <summary>
            /// The persistence mode for OPC Diagnostics log data.
            /// </summary>
            public string? OpcDiagnosticsPersistence
            {
                get => GetDynamicProperty<string>(Properties.AdminSettings.OpcDiagnostics.Persistence);
                set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.Persistence, value);
            }

            /// <summary>
            /// Maximum number of OPC Diagnostics log records before removal of oldest records.
            /// </summary>
            public int? OpcDiagnosticsMaxRecords
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MaxRecords);
                set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MaxRecords, value);
            }

            /// <summary>
            /// Directory path where OPC Diagnostics log files are stored.
            /// </summary>
            public string? OpcDiagnosticsLogFilePath
            {
                get => GetDynamicProperty<string>(Properties.AdminSettings.OpcDiagnostics.LogFilePath);
                set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.LogFilePath, value);
            }

            /// <summary>
            /// Maximum size in KB of a single OPC Diagnostics log file.
            /// </summary>
            public int? OpcDiagnosticsMaxSingleFileSizeKb
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MaxSingleFileSizeKb);
                set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MaxSingleFileSizeKb, value);
            }

            /// <summary>
            /// Minimum number of days before OPC Diagnostics log files are deleted.
            /// </summary>
            public int? OpcDiagnosticsMinDaysToPreserve
            {
                get => GetDynamicProperty<int>(Properties.AdminSettings.OpcDiagnostics.MinDaysToPreserve);
                set => SetDynamicProperty(Properties.AdminSettings.OpcDiagnostics.MinDaysToPreserve, value);
            }

            #endregion
        }
    }

}
