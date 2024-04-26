using ApiProxies.Extensions;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using System;
using System.IO;
using ScoutUtilities.Common;

namespace ApiProxies.Callbacks
{
    public class SampleImageResult :
        ApiCallbackEvent<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>,
        IApiCallback<sample_image_result_callback>
    {
        public SampleImageResult() : base(typeof(SampleImageResult).Name)
        {
            EventType = ApiEventType.WorkQueue_Image_Result;
            Callback = DoCallback;
        }

        /// <summary>
        /// Gets a delegate to the callback handling method of this IApiCallback.
        /// </summary>
        public sample_image_result_callback Callback { get; }

        /// <summary>
        /// Gets or sets a predicate that dynamically generates a fully qualified filename to be used to save the image results.
        /// If NULL, then the image will not be saved to disk.  This property is reset to NULL
        /// after each callback occurs.
        /// </summary>
        public Func<Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>, string>
            GenerateImageSaveRootDir { get; set; }

        private IntPtr CallbackArg1 { get; set; }
        private ushort CallbackArg2 { get; set; }
        private IntPtr CallbackArg3 { get; set; }
        private BasicResultAnswers CallbackArg4 { get; set; }
        private BasicResultAnswers CallbackArg5 { get; set; }

        protected override void MarshalArgsToMembers()
        {
            Results = new Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>(
                CallbackArg1.MarshalToSampleEswDomain(),
                CallbackArg2,
                CallbackArg4,
                CallbackArg3.MarshalToImageSetDto(),
                CallbackArg5);
        }

        protected override void ProcessUnmanagedData()
        {
            // Must save now because the imageDataPtr is only valid in this scope
            if (GenerateImageSaveRootDir != null && Results.Item4 != null && Results.Item1 != null)
            {
                var imagePath = Path.Combine(GenerateImageSaveRootDir(Results), ApplicationConstants.ImageFileName + "_" + Results.Item2 + ApplicationConstants.ImageFileExtension);
                Results.Item4.BrightfieldImage.SaveImage(imagePath);
                Results.Item4.FlourescenceImages.ForEach(x =>
                {
                    x.ImageSource = null;
                });

            }
        }

        private void DoCallback(IntPtr sampleDefinitionPtr, ushort imageSeqNum, IntPtr imgSetPtr,
            BasicResultAnswers cumulativeResults, BasicResultAnswers imageResults)
        {
            lock (_callbackLock)
            {
                CallbackArg1 = sampleDefinitionPtr;
                CallbackArg2 = imageSeqNum;
                CallbackArg3 = imgSetPtr;
                CallbackArg4 = cumulativeResults;
                CallbackArg5 = imageResults;

                OnCallback();
            }
        }
    }
}