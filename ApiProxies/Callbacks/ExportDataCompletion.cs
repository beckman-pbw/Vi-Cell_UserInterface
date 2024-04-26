using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class ExportDataCompletion : ApiCallbackEvent<HawkeyeError , string>, IApiCallback<export_data_completion_callback>
    {

        public ExportDataCompletion() : base(typeof(ExportDataCompletion).Name)
        {
            EventType = ApiEventType.ExportCompletion;
            Callback = DoCallback;
        }


        public export_data_completion_callback Callback { get; }
        private HawkeyeError CallbackArg1 { get; set; }
        private string CallbackArg2 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, string>(CallbackArg1, CallbackArg2);
        }


        private void DoCallback(HawkeyeError exportStatus, string completionStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = exportStatus;
                CallbackArg2 = completionStatus;
                OnCallback();
            }
        }
    }
}
