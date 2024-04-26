using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using System;

namespace ApiProxies.Callbacks
{
    public class WorkListCompleted : ApiCallbackEvent<uuidDLL>, IApiCallback<worklist_completion_callback>
    {
        public WorkListCompleted() : base(typeof(WorkListCompleted).Name)
        {
            EventType = ApiEventType.WorkQueue_Completed;
            Callback = DoCallback;
        }

        public worklist_completion_callback Callback { get; }

        private uuidDLL CallbackArg1 { get; set; }

        private void DoCallback(uuidDLL uuid)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = uuid;

                OnCallback();
            }
        }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<uuidDLL>(CallbackArg1);
        }
    }
}