using HawkeyeCoreAPI;
using ScoutDomains;

namespace ScoutModels.Dialogs
{
    public class ActiveDirectoryModel
    {
        public static ActiveDirectoryDomain GetActiveDirectorySettings()
        {
            var config = ActiveDirectory.GetActiveDirConfigAPI();
            var groups = ActiveDirectory.GetActiveDirectoryGroupMapsAPI();
            return new ActiveDirectoryDomain(config, groups);
        }

        public static bool SetActiveDirectorySettings(ActiveDirectoryDomain activeDirectoryDomain)
        {
            var result = ActiveDirectory.SetActiveDirConfigAPI(activeDirectoryDomain.ActiveDirConfig);
            result &= ActiveDirectory.SetActiveDirectoryGroupMapsAPI(activeDirectoryDomain.ActiveDirGroups);
            return result;
        }
    }
}