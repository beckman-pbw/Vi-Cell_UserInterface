using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutDomains
{
    public class ActiveDirectoryDomain
    {
        public ActiveDirectoryConfigDomain ActiveDirConfig { get; set; }
        public List<ActiveDirectoryGroupDomain> ActiveDirGroups { get; set; }

        public ActiveDirectoryDomain(string server, ushort port, string baseDn, string normalMap, string advancedMap, string adminMap)
        {
            ActiveDirConfig = new ActiveDirectoryConfigDomain(server, port, baseDn);
            ActiveDirGroups = new List<ActiveDirectoryGroupDomain>
            {
                new ActiveDirectoryGroupDomain(UserPermissionLevel.eNormal, normalMap),
                new ActiveDirectoryGroupDomain(UserPermissionLevel.eElevated, advancedMap),
                new ActiveDirectoryGroupDomain(UserPermissionLevel.eAdministrator, adminMap)
            };
        }

        public ActiveDirectoryDomain(ActiveDirectoryConfigDomain config, List<ActiveDirectoryGroupDomain> groups)
        {
            ActiveDirConfig = config;
            ActiveDirGroups = groups;
        }

        public ActiveDirectoryDomain(ActiveDirConfig config, ActiveDirGroup[] groups)
        {
            ActiveDirConfig = new ActiveDirectoryConfigDomain(config);
            ActiveDirGroups = new List<ActiveDirectoryGroupDomain>();
            foreach (var group in groups)
            {
                ActiveDirGroups.Add(new ActiveDirectoryGroupDomain(group));
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="activeDirectoryDomain"></param>
        public ActiveDirectoryDomain(ActiveDirectoryDomain activeDirectoryDomain)
        {
            ActiveDirConfig = new ActiveDirectoryConfigDomain(activeDirectoryDomain.ActiveDirConfig);
            ActiveDirGroups = new List<ActiveDirectoryGroupDomain>(activeDirectoryDomain.ActiveDirGroups);
        }

        public bool IsPopulated()
        {
            return null != ActiveDirConfig &&
                   ActiveDirConfig.Port > 0 &&
                   !string.IsNullOrEmpty(ActiveDirConfig.Server) &&
                   !string.IsNullOrEmpty(ActiveDirConfig.BaseDn) &&
                   ActiveDirGroups.Count >= 3 &&
                   ActiveDirGroups.All(adg => ! string.IsNullOrWhiteSpace(adg.ActiveDirGroupMap));
        }

        public void Add(ActiveDirectoryGroupDomain groupDomain)
        {
            ActiveDirGroups.Add(groupDomain);
        }

        public void Remove(ActiveDirectoryGroupDomain groupDomain)
        {
            ActiveDirGroups.Remove(groupDomain);
        }

        public ActiveDirConfig GetActiveDirConfig()
        {
            return ActiveDirConfig.GetActiveDirConfig();
        }

        public ActiveDirGroup[] GetActiveDirGroups()
        {
            return ActiveDirectoryGroupDomain.GetActiveDirGroupArray(ActiveDirGroups);
        }

        public string ActiveDirMapAdmin => ActiveDirGroups.FirstOrDefault(g => g.UserRole == UserPermissionLevel.eAdministrator)?.ActiveDirGroupMap;

        public override string ToString()
        {
            return ActiveDirConfig + string.Join("; ", ActiveDirGroups);
        }

        protected bool Equals(ActiveDirectoryDomain other)
        {
            return ActiveDirConfig.Equals(other.ActiveDirConfig) && !( ActiveDirGroups.Except(other.ActiveDirGroups).Any() || other.ActiveDirGroups.Except(ActiveDirGroups).Any());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals(obj as ActiveDirectoryDomain);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ActiveDirConfig != null ? ActiveDirConfig.GetHashCode() : 0) * 397) ^ (ActiveDirGroups != null ? ActiveDirGroups.GetHashCode() : 0);
            }
        }
    }

    public class ActiveDirectoryGroupDomain : IEquatable<ActiveDirectoryGroupDomain>
    {
        public UserPermissionLevel UserRole { get; set; }
        public string ActiveDirGroupMap { get; set; }

        public ActiveDirectoryGroupDomain(UserPermissionLevel userRole, string activeDirGroupMap)
        {
            UserRole = userRole;
            ActiveDirGroupMap = activeDirGroupMap;
        }

        public ActiveDirectoryGroupDomain(ActiveDirGroup activeDirGroup)
        {
            UserRole = activeDirGroup.UserRole;
            ActiveDirGroupMap = activeDirGroup.ActiveDirGroupMap.ToSystemString();
        }

        public ActiveDirGroup GetActiveDirGroup()
        {
            var activeDirGroup = new ActiveDirGroup();
            activeDirGroup.UserRole = UserRole;
            activeDirGroup.ActiveDirGroupMap = ActiveDirGroupMap.ToIntPtr();
            return activeDirGroup;
        }

        public static ActiveDirGroup[] GetActiveDirGroupArray(List<ActiveDirectoryGroupDomain> groupDomains)
        {
            var array = new ActiveDirGroup[groupDomains.Count];

            for (var i = 0; i < groupDomains.Count; i++)
            {
                array[i] = groupDomains[i].GetActiveDirGroup();
            }

            return array;
        }

        public override string ToString()
        {
            return $"{UserRole} - {ActiveDirGroupMap}";
        }

        public bool Equals(ActiveDirectoryGroupDomain other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return UserRole == other.UserRole && string.Equals(ActiveDirGroupMap, other.ActiveDirGroupMap);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ActiveDirectoryGroupDomain) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) UserRole * 397) ^ (ActiveDirGroupMap != null ? ActiveDirGroupMap.GetHashCode() : 0);
            }
        }
    }

    public class ActiveDirectoryConfigDomain : IEquatable<ActiveDirectoryConfigDomain>
    {
        public string Server { get; set; }
        public string BaseDn { get; set; }
        public ushort Port { get; set; }

        public ActiveDirectoryConfigDomain(string server, ushort port, string baseDn)
        {
            Server = server;
            Port = port;
            BaseDn = baseDn;
        }

        public ActiveDirectoryConfigDomain(ActiveDirConfig activeDirConfig)
        {
            Server = activeDirConfig.AdServer.ToSystemString();
            Port = activeDirConfig.AdPort;
            BaseDn = activeDirConfig.BaseDn.ToSystemString();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="activeDirConfig"></param>
        public ActiveDirectoryConfigDomain(ActiveDirectoryConfigDomain activeDirConfig)
        {
            Server = activeDirConfig.Server;
            Port = activeDirConfig.Port;
            BaseDn = activeDirConfig.BaseDn;
        }

        public ActiveDirConfig GetActiveDirConfig()
        {
            var config = new ActiveDirConfig();
            config.AdPort = Port;
            config.AdServer = Server.ToIntPtr();
            config.BaseDn = BaseDn.ToIntPtr();
            return config;
        }

        public override string ToString()
        {
            return $"{Server ?? "[EMPTY SERVER]"}:{Port} - {BaseDn ?? "[EMPTY BASEDN]"}";
        }

        public bool Equals(ActiveDirectoryConfigDomain other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Server, other.Server) && string.Equals(BaseDn, other.BaseDn) && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((ActiveDirectoryConfigDomain) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Server != null ? Server.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BaseDn != null ? BaseDn.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Port.GetHashCode();
                return hashCode;
            }
        }
    }
}