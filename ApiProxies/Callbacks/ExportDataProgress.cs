using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ApiProxies.Callbacks
{
    
    public class ExportDataProgress : ApiCallbackEvent<HawkeyeError,uuidDLL>, IApiCallback<export_data_progress_callback>
    {

        public ExportDataProgress() : base(typeof(ExportDataProgress).Name)
        {
            EventType = ApiEventType.ExportProgress;
            Callback = DoCallback;
        }

        public export_data_progress_callback Callback { get; }
        private HawkeyeError CallbackArg1 { get; set; }
        private uuidDLL CallbackArg2 { get; set; }

        private void DoCallback(HawkeyeError exportStatus, uuidDLL uuid)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = exportStatus;
                CallbackArg2 = uuid;
                OnCallback();
            }
        }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, uuidDLL>(CallbackArg1, CallbackArg2);
        }
    }
}
