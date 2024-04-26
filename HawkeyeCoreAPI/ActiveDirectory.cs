using System;
using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class ActiveDirectory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError ValidateActiveDirConfig(IntPtr activeDirConfigFromCall, string adminGroup, string uName, string password, out bool valid);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetActiveDirConfig(out IntPtr activeDirConfigFromCall);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetActiveDirConfig(IntPtr activeDirConfigFromCall);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetActiveDirectoryGroupMaps(out IntPtr activeDirGroupsArray, out uint count);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetActiveDirectoryGroupMaps(IntPtr activeDirGroupsArray, uint count);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeActiveDirConfig(IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeActiveDirGroupMaps(IntPtr groups, uint count);

        #endregion

        #region API_Calls
        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static bool ValidateActiveDirConfigAPI(ActiveDirectoryConfigDomain activeDirDomain, string adminGroup, string uName, string password)
        {
            var config = activeDirDomain.GetActiveDirConfig();
            var configIntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(config));
            Marshal.StructureToPtr(config, configIntPtr, false);
            var result = ValidateActiveDirConfig(configIntPtr, adminGroup, uName, password, out var valid);
            if (result != HawkeyeError.eSuccess)
            {
                return false;
            }
            Marshal.FreeCoTaskMem(configIntPtr);
            return valid;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static ActiveDirectoryConfigDomain GetActiveDirConfigAPI()
        {
            var result = GetActiveDirConfig(out var activeDirConfigIntPtr);
            if (result != HawkeyeError.eSuccess)
            {
                Log.Warn($"GetActiveDirConfigAPI: {result}");
                return null;
            }

            var activeDirConfig = (ActiveDirConfig) Marshal.PtrToStructure(activeDirConfigIntPtr, typeof(ActiveDirConfig));
            var activeDirConfigDomain = new ActiveDirectoryConfigDomain(activeDirConfig);
            
            FreeActiveDirConfig(activeDirConfigIntPtr);
            return activeDirConfigDomain;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static bool SetActiveDirConfigAPI(ActiveDirectoryConfigDomain activeDirDomain)
        {
            var config = activeDirDomain.GetActiveDirConfig();
            var configIntPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(config));
            Marshal.StructureToPtr(config, configIntPtr, false);

            var result = SetActiveDirConfig(configIntPtr);
            if (result != HawkeyeError.eSuccess)
            {
                Log.Error($"SetActiveDirConfig: {activeDirDomain}, {result}");
                return false;
            }

            Marshal.FreeCoTaskMem(configIntPtr);

            return true;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static List<ActiveDirectoryGroupDomain> GetActiveDirectoryGroupMapsAPI()
        {
            var result = GetActiveDirectoryGroupMaps(out IntPtr activeDirGroupsIntPrt, out uint count);
            if (result != HawkeyeError.eSuccess || count == 0)
            {
                Log.Warn($"GetActiveDirectoryGroupMapsAPI: {result}");
                return new List<ActiveDirectoryGroupDomain>();
            }

            var activeDirConfig = new List<ActiveDirectoryGroupDomain>();

            var activeDirGroup = new ActiveDirGroup();
            var size = Marshal.SizeOf(activeDirGroup);
            var intPtrCopy = activeDirGroupsIntPrt;
            for (var i = 0; i < count; i++)
            {
                var adg = (ActiveDirGroup) Marshal.PtrToStructure(intPtrCopy, typeof(ActiveDirGroup));
                activeDirConfig.Add(new ActiveDirectoryGroupDomain(adg));
                intPtrCopy += size;
            }

            FreeActiveDirGroupMaps(activeDirGroupsIntPrt, count);
            return activeDirConfig;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static bool SetActiveDirectoryGroupMapsAPI(List<ActiveDirectoryGroupDomain> groups)
        {
            var groupSize = Marshal.SizeOf(typeof(ActiveDirGroup));
            var allGroupsIntPtr = Marshal.AllocCoTaskMem(groups.Count * groupSize);

            for (var i = 0; i < groups.Count; i++)
            {
                var groupStruct = groups[i].GetActiveDirGroup();
                var groupPtr = allGroupsIntPtr + (i * groupSize);
                Marshal.StructureToPtr(groupStruct, groupPtr, false);
            }

            var result = SetActiveDirectoryGroupMaps(allGroupsIntPtr, (uint) groups.Count);
            Marshal.FreeCoTaskMem(allGroupsIntPtr);
            
            if (result != HawkeyeError.eSuccess)
            {
                Log.Error($"SetActiveDirectoryGroupMapsAPI:: count: {groups.Count}), {result}");
                return false;
            }

            return true;
        }

        #endregion
    }
}