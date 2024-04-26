using ScoutUtilities.Enums;
using ApiProxies.Generic;
using ApiProxies.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Delegate;

namespace ApiProxies.Callbacks
{
    public class ServiceLiveImage : ApiCallbackEvent<HawkeyeError, ImageDto, List<LiveImageDataDomain>>, IApiCallback<service_live_image_callback>
    {
        public ServiceLiveImage() : base(typeof(ServiceLiveImage).Name)
        {
            EventType = ApiEventType.Service_Live_Image;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public service_live_image_callback Callback { get; }

        private HawkeyeError CallbackArg1 { get; set; }
        private IntPtr CallbackArg2 { get; set; }

        /// <summary>
        /// Marshals injected callback argument data into local properties in the managed memory space.
        /// </summary>
        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<HawkeyeError, ImageDto, List<LiveImageDataDomain>>(CallbackArg1,
                CallbackArg2.MarshalToImageDto(), CallbackArg2.MarshalToBgUniformity());
        }

        private void DoCallback(HawkeyeError callbackStatus, IntPtr imgWrapperPtr)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = callbackStatus;
                CallbackArg2 = imgWrapperPtr;

                OnCallback();
            }
        }
    }
}