using ApiProxies.Misc;

namespace ApiProxies.Callbacks
{
    public class SampleItemCompleted : SampleStatusBase
    {
        public SampleItemCompleted() : base(typeof(SampleItemCompleted).Name)
        {
            EventType = ApiEventType.WorkQueue_Item_Completed;
        }
    }
}