using ApiProxies.Misc;

namespace ApiProxies.Callbacks
{
    public class SampleItemStatus : SampleStatusBase
    {
        public SampleItemStatus() : base(typeof(SampleItemStatus).Name)
        {
            EventType = ApiEventType.WorkQueue_Item_Status;
        }
    }
}