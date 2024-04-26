namespace ApiProxies.Commands.QueueManagement
{
    public class StopQueue : ApiCommandBase
    {
        public StopQueue()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.WorkQueue.StopQueueAPI();
        }
    }
}