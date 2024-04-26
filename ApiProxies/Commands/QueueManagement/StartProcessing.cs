using ApiProxies.Callbacks;
using ApiProxies.Generic;
using ApiProxies.Misc;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using System;

namespace ApiProxies.Commands.QueueManagement
{
    public class StartProcessing : ApiCommand<
        IApiCallback<sample_status_callback>,
        IApiCallback<sample_image_result_callback>,
        IApiCallback<sample_status_callback>,
        IApiCallback<worklist_completion_callback>, string>
    {
        public StartProcessing(string username,
            Func<Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>, string>
                fileRootPathGenerator)
        {
            ManagesMemory = false;

            Arguments = new Tuple<
                IApiCallback<sample_status_callback>,
                IApiCallback<sample_image_result_callback>,
                IApiCallback<sample_status_callback>,
                IApiCallback<worklist_completion_callback>, string>
            (
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Item_Status) as IApiCallback<sample_status_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Image_Result) as IApiCallback<sample_image_result_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Item_Completed) as IApiCallback<sample_status_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Completed) as IApiCallback<worklist_completion_callback>, 
                username
            );

            if (Arguments.Item2 is SampleImageResult imgResultCallback)
            {
                imgResultCallback.GenerateImageSaveRootDir = fileRootPathGenerator;
            }
        }

        protected override void InvokeInternal()
        {
            //@todo - remove password parameter if not needed
            Result = HawkeyeCoreAPI.WorkList.StartProcessingAPI(Arguments.Item5, "password",
                Arguments.Item1.Callback,
                Arguments.Item2.Callback,
                Arguments.Item3.Callback,
                Arguments.Item4.Callback);
        }
    }
}