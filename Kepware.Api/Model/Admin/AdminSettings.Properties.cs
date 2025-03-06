using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kepware.Api.Model
{
    public partial class Properties
    {
        public static class AdminSettings
        {
            /// <summary>
            /// Contains constants related to licensing settings.
            /// </summary>
            public static class Licensing
            {
                /// <summary>
                /// The licensing server name or IP address.
                /// </summary>
                public const string ServerName = "libadminsettings.LICENSING_SERVER_NAME";

                /// <summary>
                /// Enables or disables the licensing server connection.
                /// </summary>
                public const string ServerEnable = "libadminsettings.LICENSING_SERVER_ENABLE";

                /// <summary>
                /// The port number used to connect to license server for non-TLS connections.
                /// </summary>
                public const string ServerPort = "libadminsettings.LICENSING_SERVER_PORT";

                /// <summary>
                /// The port number used for secure (TLS) licensing server connections.
                /// </summary>
                public const string ServerSslPort = "libadminsettings.LICENSING_SERVER_SSL_PORT";

                /// <summary>
                /// Allows or disallows insecure (non-TLS) connections to the licensing server.
                /// </summary>
                public const string AllowInsecureComms = "libadminsettings.LICENSING_SERVER_ALLOW_INSECURE_COMMS";

                /// <summary>
                /// Allows the use of self-signed certificates for licensing server communication.
                /// </summary>
                public const string AllowSelfSignedCerts = "libadminsettings.LICENSING_SERVER_ALLOW_SELF_SIGNED_CERTS";

                /// <summary>
                /// Custom client alias used for requesting licenses from the server.
                /// </summary>
                public const string ClientAlias = "libadminsettings.LICENSING_CLIENT_ALIAS";

                /// <summary>
                /// Forces an immediate license recheck.
                /// </summary>
                public const string ForceRecheck = "libadminsettings.LICENSING_FORCE_RECHECK";

                /// <summary>
                /// The time interval (in minutes) between automatic license state checks.
                /// </summary>
                public const string RecheckIntervalMinutes = "libadminsettings.LICENSING_CHECK_PERIOD_MINS";
            }

            /// <summary>
            /// Contains constants related to event log settings.
            /// </summary>
            public static class EventLog
            {
                /// <summary>
                /// Specify the TCP/IP port number that should be used for the event log. You may need to configure your network firewall settings to permit communication on this port.
                /// </summary>
                public const string ConnectionPort = "libadminsettings.EVENT_LOG_CONNECTION_PORT";

                /// <summary>
                /// Specify the persistence mode to use for event log records.
                /// </summary>
                public const string Persistence = "libadminsettings.EVENT_LOG_PERSISTENCE";

                /// <summary>
                /// Specify the number of records the log can contain. Once reached, oldest records will be discarded.
                /// </summary>
                public const string MaxRecords = "libadminsettings.EVENT_LOG_MAX_RECORDS";

                /// <summary>
                /// Specify the directory where log files will be stored.
                /// </summary>
                public const string LogFilePath = "libadminsettings.EVENT_LOG_LOG_FILE_PATH";

                /// <summary>
                /// Specify the maximum size in KB that any one log file can contain.
                /// </summary>
                public const string MaxSingleFileSizeKb = "libadminsettings.EVENT_LOG_MAX_SINGLE_FILE_SIZE_KB";

                /// <summary>
                /// A log file is deleted when the newest record it contains is older than the specified value.
                /// </summary>
                public const string MinDaysToPreserve = "libadminsettings.EVENT_LOG_MIN_DAYS_TO_PRESERVE";

                /// <summary>
                /// Print log messages to the console window.
                /// </summary>
                public const string LogToConsole = "libadminsettings.EVENT_LOG_LOG_TO_CONSOLE";
            }

            /// <summary>
            /// Contains constants related to OPC diagnostics.
            /// </summary>
            public static class OpcDiagnostics
            {
                /// <summary>
                /// Specify the persistence mode to use for OPC Diagnostics records.
                /// </summary>
                public const string Persistence = "libadminsettings.OPC_DIAGS_PERSISTENCE";

                /// <summary>
                /// Specify the number of records the log can contain. Once reached, oldest records will be discarded.
                /// </summary>
                public const string MaxRecords = "libadminsettings.OPC_DIAGS_MAX_RECORDS";

                /// <summary>
                /// Specify the directory where log files will be stored.
                /// </summary>
                public const string LogFilePath = "libadminsettings.OPC_DIAGS_LOG_FILE_PATH";

                /// <summary>
                /// Specify the maximum size in KB that any one log file can contain.
                /// </summary>
                public const string MaxSingleFileSizeKb = "libadminsettings.OPC_DIAGS_MAX_SINGLE_FILE_SIZE_KB";

                /// <summary>
                /// A log file is deleted when the newest record it contains is older than the specified value.
                /// </summary>
                public const string MinDaysToPreserve = "libadminsettings.OPC_DIAGS_MIN_DAYS_TO_PRESERVE";
            }

            /// <summary>
            /// Contains constants related to communication diagnostics.
            /// </summary>
            public static class CommDiagnostics
            {
                /// <summary>
                /// Specify the persistence mode to use for Communications Diagnostics records.
                /// </summary>
                public const string Persistence = "libadminsettings.COMM_DIAGS_PERSISTENCE";

                /// <summary>
                /// Specify the number of records the log can contain. Once reached, oldest records will be discarded.
                /// </summary>
                public const string MaxRecords = "libadminsettings.COMM_DIAGS_MAX_RECORDS";

                /// <summary>
                /// Specify the directory where log files will be stored.
                /// </summary>
                public const string LogFilePath = "libadminsettings.COMM_DIAGS_LOG_FILE_PATH";

                /// <summary>
                /// Specify the maximum size in KB that any one log file can contain.
                /// </summary>
                public const string MaxSingleFileSizeKb = "libadminsettings.COMM_DIAGS_MAX_SINGLE_FILE_SIZE_KB";

                /// <summary>
                /// A log file is deleted when the newest record it contains is older than the specified value.
                /// </summary>
                public const string MinDaysToPreserve = "libadminsettings.COMM_DIAGS_MIN_DAYS_TO_PRESERVE";
            }

            /// <summary>
            /// Contains constants related to Configuration API diagnostics.
            /// </summary>
            public static class ConfigApi
            {
                /// <summary>
                /// Specify the persistence mode to use for Configuration API records.
                /// </summary>
                public const string Persistence = "libadminsettings.CONFIG_API_PERSISTENCE";

                /// <summary>
                /// Specify the number of records the log can contain. Once reached, oldest records will be discarded.
                /// </summary>
                public const string MaxRecords = "libadminsettings.CONFIG_API_MAX_RECORDS";

                /// <summary>
                /// Specify the directory where log files will be stored.
                /// </summary>
                public const string LogFilePath = "libadminsettings.CONFIG_API_LOG_FILE_PATH";

                /// <summary>
                /// Specify the maximum size in KB that any one log file can contain.
                /// </summary>
                public const string MaxSingleFileSizeKb = "libadminsettings.CONFIG_API_MAX_SINGLE_FILE_SIZE_KB";

                /// <summary>
                /// A log file is deleted when the newest record it contains is older than the specified value.
                /// </summary>
                public const string MinDaysToPreserve = "libadminsettings.CONFIG_API_MIN_DAYS_TO_PRESERVE";
            }

        }
    }
}