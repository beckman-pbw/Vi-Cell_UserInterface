namespace ApiProxies.Commands.Service
{
    public class StopLiveImageFeed : ApiCommandBase
    {
        public StopLiveImageFeed()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.StopLiveImageFeedAPI();
        }
    }
}