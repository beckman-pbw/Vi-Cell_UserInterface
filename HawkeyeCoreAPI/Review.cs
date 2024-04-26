using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using ScoutUtilities.Delegate;

namespace HawkeyeCoreAPI
{
    public static partial class Review
    {
        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError ReanalyzeSample(
            uuidDLL sample_id, 
            UInt32 cellType_index,
            UInt32 analysis_index,
            bool fromImages, [MarshalAs(UnmanagedType.FunctionPtr)] sample_analysis_callback onSampleComplete);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ReanalyzeSampleAPI(
            uuidDLL sample_id, 
            uint cellType_index, 
            uint analysis_index,
            bool fromImages,
            sample_analysis_callback onSampleComplete)
        {
            return ReanalyzeSample(sample_id, cellType_index, analysis_index, fromImages, onSampleComplete);
        }

        #endregion


        #region Private Methods


        #endregion

    }
}
