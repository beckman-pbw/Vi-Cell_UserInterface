using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class DeleteSampleResult : ApiCallbackEvent<HawkeyeError, uuidDLL>, IApiCallback<delete_sample_record_callback>
    {
        public DeleteSampleResult() : base(typeof(DeleteSampleResult).Name)
        {
            EventType = ApiEventType.Delete_Sample_Results;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public delete_sample_record_callback Callback { get; }

        private HawkeyeError CallbackArg1 { get; set; }
        private uuidDLL CallbackArg2 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, uuidDLL>(CallbackArg1, CallbackArg2);
        }

        private void DoCallback(HawkeyeError deleteResult, uuidDLL uuid)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = deleteResult;
                CallbackArg2 = uuid;

                OnCallback();
            }
        }
    }
}