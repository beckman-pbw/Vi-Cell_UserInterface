using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Enums;
using System;
using System.Diagnostics.CodeAnalysis;
using ScoutUtilities.Delegate;
using ScoutDomains;

namespace ApiProxies.Callbacks
{
    public class AutofocusState : ApiCallbackEvent<eAutofocusState, AutofocusResultsDto>, IApiCallback<autofocus_state_callback_t>
    {
        public AutofocusState() : base(typeof(AutofocusState).Name)
        {
            EventType = ApiEventType.Autofocus_State;
            Callback = DoCallback;
        }

        public autofocus_state_callback_t Callback { get; }

        public string SaveImageToFilename { get; set; }

        private eAutofocusState CallbackArg1 { get; set; }
        private IntPtr CallbackArg2 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<eAutofocusState, AutofocusResultsDto>(CallbackArg1,
                CallbackArg2.MarshalToAutofocusResultsDto());
        }

        private void DoCallback(eAutofocusState autofocusState, IntPtr autofocusResults)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = autofocusState;
                CallbackArg2 = autofocusResults;

                OnCallback();
            }
        }
    }
}