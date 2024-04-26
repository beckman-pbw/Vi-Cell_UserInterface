using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class ColumnSettingsApi
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetSampleColumns(string username, out IntPtr columnSettingsArray, out uint count);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetSampleColumns(string username, IntPtr columnSettingsArray, uint count);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeSampleColumns(IntPtr columnSettingsArray, uint count);

        #endregion

        #region API_Calls

        public static List<ColumnSetting> GetSampleColumnsApi(string username)
        {
            var list = new List<ColumnSetting>();

            try
            {
                var result = GetSampleColumns(username, out var columnSettingsArrayIntPtr, out var count);
                if (result != HawkeyeError.eSuccess || count == 0)
                {
                    Log.Warn($"{nameof(GetSampleColumns)} error: {result} - count: {count}");
                    return list;
                }

                var columnSettingSize = Marshal.SizeOf(new ColumnSetting());
                var intPtrCopy = columnSettingsArrayIntPtr;
                for (var i = 0; i < count; i++)
                {
                    var adg = (ColumnSetting)Marshal.PtrToStructure(intPtrCopy, typeof(ColumnSetting));
                    list.Add(adg);
                    intPtrCopy += columnSettingSize;
                }

                FreeSampleColumns(columnSettingsArrayIntPtr, count);
            }
            catch (Exception e)
            {
                Log.Error($"GetSampleColumnsApi: ", e);
            }
            
            return list;
        }

        public static bool SetSampleColumnsApi(string username, List<ColumnSetting> columnSettings)
        {
            try
            {
                var groupSize = Marshal.SizeOf(typeof(ColumnSetting));
                var allColSettingsIntPtr = Marshal.AllocCoTaskMem(columnSettings.Count * groupSize);

                for (var i = 0; i < columnSettings.Count; i++)
                {
                    var colSettingStruct = columnSettings[i];
                    var colSettingPtr = allColSettingsIntPtr + (i * groupSize);
                    Marshal.StructureToPtr(colSettingStruct, colSettingPtr, false);
                }

                var result = SetSampleColumns(username, allColSettingsIntPtr, (uint) columnSettings.Count);
                Marshal.FreeCoTaskMem(allColSettingsIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"SetSampleColumnsApi:: count: {columnSettings.Count}: {result}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"SetSampleColumnsApi::", e);
                return false;
            }
        }

        #endregion
    }
}