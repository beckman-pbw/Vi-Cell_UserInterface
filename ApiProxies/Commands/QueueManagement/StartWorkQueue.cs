using ApiProxies.Callbacks;
using ApiProxies.Generic;
using ScoutDomains.DataTransferObjects;
using ApiProxies.Misc;
using ScoutUtilities.Delegate;
using ScoutUtilities.Structs;
using System;
using ScoutDomains.EnhancedSampleWorkflow;

namespace ApiProxies.Commands.QueueManagement
{
    public class StartWorkQueue : ApiCommand<
        IApiCallback<sample_status_callback>,
        IApiCallback<sample_status_callback>, 
        IApiCallback<worklist_completion_callback>,
        IApiCallback<sample_image_result_callback>>
    {
        public StartWorkQueue(Func<Tuple<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>, string> fileRootPathGenerator)
        {
            ManagesMemory = false;

            Arguments = new Tuple<IApiCallback<sample_status_callback>,
                IApiCallback<sample_status_callback>,
                IApiCallback<worklist_completion_callback>,
                IApiCallback<sample_image_result_callback>>
            (
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Item_Status) as IApiCallback<sample_status_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Item_Completed) as IApiCallback<sample_status_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Completed) as IApiCallback<worklist_completion_callback>,
                ApiEventBroker.Instance.GetProxyForApiEvent(ApiEventType.WorkQueue_Image_Result) as IApiCallback<sample_image_result_callback>
            );

            if (Arguments.Item4 is SampleImageResult imgResultCallback)
            {
                imgResultCallback.GenerateImageSaveRootDir = fileRootPathGenerator;
            }
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.WorkQueue.StartQueueAPI(Arguments.Item1.Callback, Arguments.Item2.Callback,
                Arguments.Item3.Callback, Arguments.Item4.Callback);
        }
    }
}