using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public class SmtpSettingsApi
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetSMTPConfig(out IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetSMTPConfig(IntPtr config);

        [DllImport(ApplicationConstants.DllName)]
        static extern void FreeSMTPConfig(IntPtr config);

        #endregion

        #region API_Calls

        public SmtpConfig GetSmtpConfigApi()
        {
            try
            {
                var result = GetSMTPConfig(out var smtpConfigIntPtr);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"Failed to get SMTP config: {result}. Returning default object.");
                    return new SmtpConfig();
                }

                var smtpConfig = (SmtpConfig)Marshal.PtrToStructure(smtpConfigIntPtr, typeof(SmtpConfig));
                FreeSMTPConfig(smtpConfigIntPtr);
                return smtpConfig;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get SMTP config from the backend", e);
                return new SmtpConfig();
            }
        }

        public bool SetSmtpConfigApi(SmtpConfig smtpConfig)
        {
            try
            {
                var size = Marshal.SizeOf(typeof(SmtpConfig));
                var smtpConfigIntPtr = Marshal.AllocCoTaskMem(size);
                Marshal.StructureToPtr(smtpConfig, smtpConfigIntPtr, false);
                var result = SetSMTPConfig(smtpConfigIntPtr);
                Marshal.FreeCoTaskMem(smtpConfigIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"Failed to set SMTP Config: {result}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to set SMTP Config", e);
                return false;
            }
        }

        #endregion
    }
}