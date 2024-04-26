using System.Runtime.InteropServices;
using JetBrains.Annotations;
using System;
using ScoutUtilities.Enums;
using ScoutUtilities.Delegate;

namespace HawkeyeCoreAPI
{
    public static partial class ExportData
    {
        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ExportInstrumentData(string username, string password, IntPtr rs_uuid_list, UInt32 num_uuid,
            string export_location_directory, eExportImages exportImages, UInt16 export_nth_image,
            export_data_completion_callback onExportCompletionCb, export_data_progress_callback exportProgressCb);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError CancelExportData();

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ExportInstrumentDataAPI(string username, string password, IntPtr rs_uuid_list, uint uuidCount,
            string export_location_directory, eExportImages exportImages, UInt16 export_nth_image,
             export_data_progress_callback exportProgressCb, export_data_completion_callback onExportCompletionCb)
        {
            return ExportInstrumentData(username, password, rs_uuid_list, uuidCount, export_location_directory, exportImages, export_nth_image,
                onExportCompletionCb, exportProgressCb);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError CancelExportDataAPI()
        {
            return CancelExportData();
        }

        #endregion


        #region Private Methods


        #endregion

    }
}
