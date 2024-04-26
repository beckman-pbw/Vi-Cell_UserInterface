using ApiProxies.Generic;
using ScoutDomains;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using ScoutDomains.RunResult;

namespace ApiProxies.Callbacks
{
    public class SampleAnalysis : ApiCallbackEvent<HawkeyeError, uuidDLL, ResultRecordDomain>,
        IApiCallback<sample_analysis_callback>
    {
        /// <summary>
        /// Parameterless default constructor required for creation using reflection.
        /// </summary>
        public SampleAnalysis() : base(typeof(SampleAnalysis).Name)
        {
            EventType = ApiEventType.Sample_Analysis;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public sample_analysis_callback Callback { get; }

        private HawkeyeError CallbackArg1 { get; set; }
        private uuidDLL CallbackArg2 { get; set; }
        private IntPtr CallbackArg3 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, uuidDLL, ResultRecordDomain>(
                CallbackArg1,
                CallbackArg2,
                CallbackArg3.MarshalToResultRecordDomain());
        }

        private void DoCallback(HawkeyeError error, uuidDLL sampleId, IntPtr resultRecordPtr)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = error;
                CallbackArg2 = sampleId;
                CallbackArg3 = resultRecordPtr;

                OnCallback();
            }
        }
    }
}
