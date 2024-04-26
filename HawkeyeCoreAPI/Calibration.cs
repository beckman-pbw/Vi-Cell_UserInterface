using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HawkeyeCoreAPI
{
    public static partial class Calibration
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        static extern void FreeCalibrationHistoryEntry(IntPtr entries, UInt32 num_entries);


        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveCalibrationActivityLogRange(
            calibration_type cal_type,
            UInt64 starttime, 
            UInt64 endtime, 
            out UInt32 num_entries, 
            out IntPtr log_entries);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError RetrieveCalibrationActivityLog(
            calibration_type cal_type,
            out UInt32 num_entries, 
            out IntPtr log_entries);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetConcentrationCalibration(
            calibration_type calType,
            double slope,
            double intercept, 
            UInt32 cal_image_count,
            uuidDLL queue_id, 
            UInt16 num_consumables, 
            IntPtr consumablesid);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ClearCalibrationActivityLog(calibration_type cal_type, UInt64 archive_prior_to_time, string verification_password, bool clearAllACupData);

        #endregion

        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetConcentrationCalibrationAPI(calibration_type calType,double slope, double intercept, UInt32 cal_image_count,
        uuidDLL queue_id, UInt16 num_consumables, List<calibration_consumable> consumables)
        {
            var wqiSize = Marshal.SizeOf(typeof(calibration_consumable));
            IntPtr consumablePTr =
                Marshal.AllocCoTaskMem(consumables.Count * Marshal.SizeOf(typeof(calibration_consumable)));
            for (int i = 0; i < consumables.Count; i++)
            {
                var pTmp = consumablePTr + (i * wqiSize);
                Marshal.StructureToPtr(consumables[i], pTmp, false);
            }

            var HawkeyeError = SetConcentrationCalibration(calType,slope, intercept, cal_image_count, queue_id, num_consumables,
                consumablePTr);
            Marshal.FreeCoTaskMem(consumablePTr);

            return HawkeyeError;
        }


        public static Tuple<HawkeyeError, IntPtr> RetrieveCalibrationActivityLogAPI(calibration_type cal_type, out UInt32 num_entries, ref List<calibration_history_entry> log_entriesAll)
        {
            var calibrationActivityState = new calibration_history_entry();
            var size = Marshal.SizeOf(calibrationActivityState);

            IntPtr calibrationActivityLog;

            var hawkeyeError = RetrieveCalibrationActivityLog(cal_type, out num_entries, out calibrationActivityLog);

            var calibrationActivityPtr = calibrationActivityLog;
            for (var i = 0; i < num_entries; i++)
            {
                log_entriesAll.Add((calibration_history_entry)Marshal.PtrToStructure(calibrationActivityPtr, typeof(calibration_history_entry)));
                calibrationActivityPtr += size;
            }

            return new Tuple<HawkeyeError, IntPtr>(hawkeyeError, calibrationActivityLog);
        }

        public static Tuple<HawkeyeError, IntPtr> RetrieveCalibrationActivityLogRangeAPI(calibration_type cal_type,
            UInt64 startTime, UInt64 endTime, out UInt32 num_entries,
            ref List<calibration_history_entry> log_entriesAll)
        {
            var size = Marshal.SizeOf(typeof(calibration_history_entry));

            IntPtr calibrationActivityLog;

            var hawkeyeError = RetrieveCalibrationActivityLogRange(cal_type, startTime, endTime, out num_entries, out calibrationActivityLog);

            var calibrationActivityPtr = calibrationActivityLog;
            for (var i = 0; i < num_entries; i++)
            {
                log_entriesAll.Add((calibration_history_entry)Marshal.PtrToStructure(calibrationActivityPtr, typeof(calibration_history_entry)));
                calibrationActivityPtr += size;
            }

            return new Tuple<HawkeyeError, IntPtr>(hawkeyeError, calibrationActivityLog);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ClearCalibrationActivityLogAPI(calibration_type cal_type, UInt64 archive_prior_to_time, string password, bool clearAllACupData)
        {
            return ClearCalibrationActivityLog(cal_type, archive_prior_to_time, password, clearAllACupData);
        }


        public static void FreeCalibrationHistoryEntryAPI(IntPtr entries, UInt32 num_entries)
        {
            if (entries == IntPtr.Zero)
            {
#if _DEBUG
                Log.Warn("FreeCalibrationHistoryEntryAPI: pointer IntPtr.Zero");
#endif
            }
            else
            {
                FreeCalibrationHistoryEntry(entries, num_entries);
            }
        }

        #endregion

    }


}
