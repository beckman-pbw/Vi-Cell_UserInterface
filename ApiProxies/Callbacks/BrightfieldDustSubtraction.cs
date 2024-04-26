using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ApiProxies.Extensions;
using ScoutUtilities.Delegate;
using ScoutDomains;

namespace ApiProxies.Callbacks
{
    public class BrightfieldDustSubtraction :
        ApiCallbackEvent<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>>,
        IApiCallback<brightfield_dustsubtraction_callback>
    {
        public BrightfieldDustSubtraction() : base(typeof(BrightfieldDustSubtraction).Name)
        {
            EventType = ApiEventType.Brightfield_Dust_Subtraction;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public brightfield_dustsubtraction_callback Callback { get; }

        public string DustImagesDirPath { get; set; }
        public string DustImagesFilePrefix { get; set; }

        private eBrightfieldDustSubtractionState CallbackArg1 { get; set; }
        private IntPtr CallbackArg2 { get; set; }
        private ushort CallbackArg3 { get; set; }
        private IntPtr CallbackArg4 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<eBrightfieldDustSubtractionState, ImageDto, List<ImageDto>>(
                CallbackArg1,
                CallbackArg2.MarshalToImageDto(),
                CallbackArg4.MarshalToImageDtoList(CallbackArg3));
        }

        static bool areImagesWritten_;

        protected override void ProcessUnmanagedData()
        {
            if (CallbackArg1 == ScoutUtilities.Enums.eBrightfieldDustSubtractionState.bds_AspiratingBuffer)
            {
                areImagesWritten_ = false;
            }

            // Only write the images once.
            if (!areImagesWritten_ && CallbackArg1 == ScoutUtilities.Enums.eBrightfieldDustSubtractionState.bds_WaitingOnUserApproval)
			{
				if (!string.IsNullOrEmpty(DustImagesDirPath))
				{
					if (!string.IsNullOrEmpty(DustImagesFilePrefix))
					{
                        areImagesWritten_ = true;
                        Results.Item3?.SaveImages(DustImagesDirPath, DustImagesFilePrefix);
					}
				}
			}
		}

		private void DoCallback(eBrightfieldDustSubtractionState status, IntPtr finalDustImg, ushort imageCount,
            IntPtr imageArray)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = status;
                CallbackArg2 = finalDustImg;
                CallbackArg3 = imageCount;
                CallbackArg4 = imageArray;

                OnCallback();
            }
        }
    }
}