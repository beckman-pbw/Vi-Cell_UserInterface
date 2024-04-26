using System;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;

namespace ApiProxies.Callbacks
{
    public class FlowcellFlushStatus : ApiCallbackEvent<eFlushFlowCellState>, IApiCallback<flowcell_flush_status_callback>
    {
        public FlowcellFlushStatus() : base(typeof(FlowcellFlushStatus).Name)
        {
            EventType = ApiEventType.Flowcell_Flush_Status;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public flowcell_flush_status_callback Callback { get; }

        private eFlushFlowCellState CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<eFlushFlowCellState>(CallbackArg1);
        }

        private void DoCallback(eFlushFlowCellState flushStatus)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = flushStatus;

                OnCallback();
            }
        }
    }
}