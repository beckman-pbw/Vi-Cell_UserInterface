using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class DatabaseSettingsApi
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetDBConfig(out IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetDBConfig(IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeDBConfig(IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        static extern HawkeyeError SetDbBackupUserPassword(string password);

        [DllImport(ApplicationConstants.DllName)]
        static extern HawkeyeError SetOpticalHardwareConfig(OpticalHardwareConfig type);

        [DllImport(ApplicationConstants.DllName)]
        static extern OpticalHardwareConfig GetOpticalHardwareConfig();

        #endregion

        #region API_Calls

        public bool SetOpticalHardwareConfigAPI(OpticalHardwareConfig type)
        {
            try
            {
                var result = SetOpticalHardwareConfig(type);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Error($"SetOpticalHardwareConfigAPI: {result}.");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"SetOpticalHardwareConfigAPI:", e);
                return false;
            }
        }

        public OpticalHardwareConfig GetOpticalHardwareConfigAPI()
        {
            try
            {
                var result = GetOpticalHardwareConfig();
                return result;
            }
            catch (Exception e)
            {
                Log.Error($"Exception GetOpticalHardwareConfigAPI.", e);
                return OpticalHardwareConfig.UNKNOWN;
            }
        }

        public bool SetDbBackupUserPasswordAPI(string password)
        {
            try
            {
                var result = SetDbBackupUserPassword(password);
                if(result != HawkeyeError.eSuccess)
                {
                    Log.Error($"SetDbBackupUserPasswordAPI: {result}.");
                    return false;
                }
                return true;
            }
            catch(Exception e)
            {
                Log.Error($"SetDbBackupUserPasswordAPI:", e);
                return false;
            }
        }

        public DbConfig GetDbConfigApi()
        {
            try
            {
                var result = GetDBConfig(out var dbConfigIntPtr);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Error($"GetDbConfigApi: {result}, returning default object.");
                    return new DbConfig();
                }

                var dbConfig = (DbConfig) Marshal.PtrToStructure(dbConfigIntPtr, typeof(DbConfig));
                FreeDBConfig(dbConfigIntPtr);
                return dbConfig;
            }
            catch (Exception e)
            {
                Log.Error($"GetDbConfigApi:", e);
                return new DbConfig();
            }
        }

        public bool SetDbConfigApi(DbConfig dbConfig)
        {
            try
            {
                var size = Marshal.SizeOf(typeof(DbConfig));
                var dbConfigIntPtr = Marshal.AllocCoTaskMem(size);
                Marshal.StructureToPtr(dbConfig, dbConfigIntPtr, false);
                var result = SetDBConfig(dbConfigIntPtr);
                Marshal.FreeCoTaskMem(dbConfigIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"SetDbConfigApi: {result}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"SetDbConfigApi:", e);
                return false;
            }
        }

        #endregion
    }
}