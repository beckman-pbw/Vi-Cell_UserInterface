using HawkeyeCoreAPI;

namespace ApiProxies.Commands.QueueManagement
{
    public class PauseQueue : ApiCommandBase
    {
        public PauseQueue()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = WorkQueue.PauseQueueAPI();
        }
    }
}