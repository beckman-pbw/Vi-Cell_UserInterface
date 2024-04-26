using System;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class FlowcellDecontaminateStatus : ApiCallbackEvent<eDecontaminateFlowCellState>, IApiCallback<flowcell_decontaminate_status_callback>
    {
        public FlowcellDecontaminateStatus() : base(typeof(FlowcellDecontaminateStatus).Name)
        {
            EventType = ApiEventType.Flowcell_Decontaminate_Status;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public flowcell_decontaminate_status_callback Callback { get; }

        private eDecontaminateFlowCellState CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<eDecontaminateFlowCellState>(CallbackArg1);
        }

        private void DoCallback(eDecontaminateFlowCellState decontaminateStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = decontaminateStatus;

                OnCallback();
            }
        }
    }
}