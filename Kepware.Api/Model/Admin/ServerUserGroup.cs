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
    /// Represents a user group in Kepware, allowing management of permissions and access control.
    /// </summary>
    [Endpoint("/config/v1/admin/server_usergroups/{name}")]
    public class ServerUserGroup : NamedEntity
    {
        /// <summary>
        /// Enables or disables the user group.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? Enabled
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.Enabled);
            set => SetDynamicProperty(Properties.ServerUserGroup.Enabled, value);
        }

        #region I/O Tag Access

        /// <summary>
        /// Allows or denies read access to I/O tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? IoTagRead
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.IoTagRead);
            set => SetDynamicProperty(Properties.ServerUserGroup.IoTagRead, value);
        }

        /// <summary>
        /// Allows or denies write access to I/O tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? IoTagWrite
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.IoTagWrite);
            set => SetDynamicProperty(Properties.ServerUserGroup.IoTagWrite, value);
        }

        /// <summary>
        /// Allows or denies dynamic addressing of I/O tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? IoTagDynamicAddressing
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.IoTagDynamicAddressing);
            set => SetDynamicProperty(Properties.ServerUserGroup.IoTagDynamicAddressing, value);
        }

        #endregion

        #region System Tag Access

        /// <summary>
        /// Allows or denies read access to system tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? SystemTagRead
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.SystemTagRead);
            set => SetDynamicProperty(Properties.ServerUserGroup.SystemTagRead, value);
        }

        /// <summary>
        /// Allows or denies write access to system tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? SystemTagWrite
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.SystemTagWrite);
            set => SetDynamicProperty(Properties.ServerUserGroup.SystemTagWrite, value);
        }

        #endregion

        #region Internal Tag Access

        /// <summary>
        /// Allows or denies read access to internal tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? InternalTagRead
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.InternalTagRead);
            set => SetDynamicProperty(Properties.ServerUserGroup.InternalTagRead, value);
        }

        /// <summary>
        /// Allows or denies write access to internal tags.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? InternalTagWrite
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.InternalTagWrite);
            set => SetDynamicProperty(Properties.ServerUserGroup.InternalTagWrite, value);
        }

        #endregion

        #region Server Permissions

        /// <summary>
        /// Allows or denies access to the licensing manager.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ManageLicenses
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ManageLicenses);
            set => SetDynamicProperty(Properties.ServerUserGroup.ManageLicenses, value);
        }

        /// <summary>
        /// Allows or denies modification of server settings.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ModifyServerSettings
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ModifyServerSettings);
            set => SetDynamicProperty(Properties.ServerUserGroup.ModifyServerSettings, value);
        }

        /// <summary>
        /// Allows or denies disconnecting clients from the server.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? DisconnectClients
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.DisconnectClients);
            set => SetDynamicProperty(Properties.ServerUserGroup.DisconnectClients, value);
        }

        /// <summary>
        /// Allows or denies replacing the running project with a new one.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ReplaceRuntimeProject
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ReplaceRuntimeProject);
            set => SetDynamicProperty(Properties.ServerUserGroup.ReplaceRuntimeProject, value);
        }

        /// <summary>
        /// Allows or denies resetting the event log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ResetEventLog
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ResetEventLog);
            set => SetDynamicProperty(Properties.ServerUserGroup.ResetEventLog, value);
        }

        /// <summary>
        /// Allows or denies resetting the OPC diagnostics log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ResetOpcDiagsLog
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ResetOpcDiagsLog);
            set => SetDynamicProperty(Properties.ServerUserGroup.ResetOpcDiagsLog, value);
        }

        /// <summary>
        /// Allows or denies resetting the communications diagnostics log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ResetCommDiagsLog
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ResetCommDiagsLog);
            set => SetDynamicProperty(Properties.ServerUserGroup.ResetCommDiagsLog, value);
        }

        /// <summary>
        /// Allows or denies access to OPC UA or XI configuration settings.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? OpcUaConfiguration
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.OpcUaConfiguration);
            set => SetDynamicProperty(Properties.ServerUserGroup.OpcUaConfiguration, value);
        }

        /// <summary>
        /// Allows or denies access to the Configuration API Transaction Log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ConfigApiLogAccess
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ConfigApiLogAccess);
            set => SetDynamicProperty(Properties.ServerUserGroup.ConfigApiLogAccess, value);
        }

        /// <summary>
        /// Allows or denies viewing security messages in the event log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ViewEventLogSecurity
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ViewEventLogSecurity);
            set => SetDynamicProperty(Properties.ServerUserGroup.ViewEventLogSecurity, value);
        }

        /// <summary>
        /// Allows or denies viewing error messages in the event log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ViewEventLogError
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ViewEventLogError);
            set => SetDynamicProperty(Properties.ServerUserGroup.ViewEventLogError, value);
        }

        /// <summary>
        /// Allows or denies viewing warning messages in the event log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ViewEventLogWarning
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ViewEventLogWarning);
            set => SetDynamicProperty(Properties.ServerUserGroup.ViewEventLogWarning, value);
        }

        /// <summary>
        /// Allows or denies viewing informational messages in the event log.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ViewEventLogInfo
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ViewEventLogInfo);
            set => SetDynamicProperty(Properties.ServerUserGroup.ViewEventLogInfo, value);
        }

        #endregion

        #region Project Modification Permissions

        /// <summary>
        /// Allows or denies adding objects to the project.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ProjectModificationAdd
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ProjectModificationAdd);
            set => SetDynamicProperty(Properties.ServerUserGroup.ProjectModificationAdd, value);
        }

        /// <summary>
        /// Allows or denies editing objects in the project.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ProjectModificationEdit
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ProjectModificationEdit);
            set => SetDynamicProperty(Properties.ServerUserGroup.ProjectModificationEdit, value);
        }

        /// <summary>
        /// Allows or denies deleting objects in the project.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? ProjectModificationDelete
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.ProjectModificationDelete);
            set => SetDynamicProperty(Properties.ServerUserGroup.ProjectModificationDelete, value);
        }

        #endregion

        #region Browse Project Namespace

        /// <summary>
        /// Allows or denies browsing of the project namespace.
        /// </summary>
        [YamlIgnore, JsonIgnore]
        public bool? BrowseNamespace
        {
            get => GetDynamicProperty<bool>(Properties.ServerUserGroup.BrowseNamespace);
            set => SetDynamicProperty(Properties.ServerUserGroup.BrowseNamespace, value);
        }

        #endregion
    }

    [Endpoint("/config/v1/admin/server_usergroups")]
    public class ServerUserGroupCollection : EntityCollection<ServerUserGroup>
    {

    }
}
