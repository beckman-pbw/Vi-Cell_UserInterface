namespace ApiProxies.Commands.QueueManagement
{
    public class ResumeQueue : ApiCommandBase
    {
        public ResumeQueue()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.WorkQueue.ResumeQueueAPI();
        }
    }
}