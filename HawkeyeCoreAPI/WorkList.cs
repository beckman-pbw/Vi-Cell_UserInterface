using JetBrains.Annotations;
using log4net;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Common;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using System;
using System.Runtime.InteropServices;
using Struct = ScoutUtilities.Structs;

namespace HawkeyeCoreAPI
{
    public static class WorkList
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetWorklist(Struct.WorkList wl);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError StartProcessing(string username, string password,
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_status_callback onSampleStatusUpdated,
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_image_result_callback onSampleImageProcessed,
            [MarshalAs(UnmanagedType.FunctionPtr)] sample_status_callback onSampleCompleted,
            [MarshalAs(UnmanagedType.FunctionPtr)] worklist_completion_callback onWorkListCompleted);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetCurrentSample(out Struct.SampleDefinition sampleDefinition);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError PauseProcessing(string username, string password);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError StopProcessing(string username, string password);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError ResumeProcessing(string username, string password);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError AddSampleSet(Struct.SampleSet sampleSet);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError CancelSampleSet(ushort sampleSetIndex);

      
        #endregion

        #region API_Calls

        public static HawkeyeError SetWorkListAPI(WorkListDomain workListDomain)
        {
            var hawkeyeError = HawkeyeError.eInvalidArgs;
            try
            {
                ApiHelper.AllocateWorkListMemory(workListDomain, out var wlStruct, out var memToFree);
                hawkeyeError = SetWorklist(wlStruct);
                ApiHelper.FreeMemory(memToFree);
                return hawkeyeError;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to SetWorkList:{Environment.NewLine}{workListDomain}", e);
                return hawkeyeError;
            }
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError StartProcessingAPI(
            string username, string password,
            sample_status_callback onSampleStatusUpdated,
            sample_image_result_callback onSampleImageProcessed,
            sample_status_callback onSampleCompleted,
            worklist_completion_callback onWorkListCompleted)
        {
            var result = StartProcessing(username, password, onSampleStatusUpdated, onSampleImageProcessed,
                onSampleCompleted, onWorkListCompleted);
            return result;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError StopProcessingAPI(string username, string password)
        {
            var result = StopProcessing(username, password);
            return result;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError PauseProcessingAPI(string username, string password)
        {
            var result = PauseProcessing(username, password); 
            return result;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError ResumeProcessingAPI(string username, string password)
        {
            var result = ResumeProcessing(username, password);
            return result;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError AddSampleSetAPI(SampleSetDomain sampleSetDomain)
        {
            ApiHelper.AllocateSampleSetMemory(sampleSetDomain, out var ssStruct, out var memToFree);
            var hawkeyeError = AddSampleSet(ssStruct);
            ApiHelper.FreeMemory(memToFree);
            return hawkeyeError;
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static HawkeyeError CancelSampleSetAPI(ushort sampleSetIndex)
        {
            var hawkeyeError = CancelSampleSet(sampleSetIndex); 
            return hawkeyeError;
        }

        #endregion
    }
}