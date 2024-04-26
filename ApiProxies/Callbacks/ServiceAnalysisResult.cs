using ApiProxies.Generic;
using ScoutDomains.DataTransferObjects;
using ApiProxies.Misc;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using ScoutUtilities.Delegate;
using ScoutDomains;

namespace ApiProxies.Callbacks
{
    public class ServiceAnalysisResult : ApiCallbackEvent<HawkeyeError, BasicResultAnswers, ImageDto>, IApiCallback<service_analysis_result_callback>
    {
        public ServiceAnalysisResult() : base(typeof(ServiceAnalysisResult).Name)
        {
            EventType = ApiEventType.Service_Analysis_Result;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public service_analysis_result_callback Callback { get; }

        private HawkeyeError CallbackArg1 { get; set; }
        private BasicResultAnswers CallbackArg2 { get; set; }
        private IntPtr CallbackArg3 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, BasicResultAnswers, ImageDto>(
                CallbackArg1,
                CallbackArg2,
                CallbackArg3.MarshalToImageDto());
        }

        private void DoCallback(HawkeyeError callbackStatus, BasicResultAnswers basicResultAnswers,
            IntPtr imgWrapperPtr)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = callbackStatus;
                CallbackArg2 = basicResultAnswers;
                CallbackArg3 = imgWrapperPtr;

                OnCallback();
            }
        }
    }
}