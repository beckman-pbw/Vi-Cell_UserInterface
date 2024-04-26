using ApiProxies.Generic;
using ApiProxies.Misc;
using System;
using System.Diagnostics.CodeAnalysis;
using ScoutUtilities.Delegate;

namespace ApiProxies.Callbacks
{
    public class AutofocusTimerCountdown : ApiCallbackEvent<UInt32>, IApiCallback<countdown_timer_callback_t>
    {
        public AutofocusTimerCountdown() : base(typeof(AutofocusTimerCountdown).Name)
        {
            EventType = ApiEventType.Autofocus_Countdown_Timer;
            Callback = DoCallback;
        }

        public countdown_timer_callback_t Callback { get; }

        private UInt32 CallbackArg1 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<UInt32>(CallbackArg1);
        }

        private void DoCallback(UInt32 secondsRemaining)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = secondsRemaining;

                OnCallback();
            }
        }
    }
}