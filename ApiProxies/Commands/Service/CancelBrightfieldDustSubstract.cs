using ScoutUtilities;

namespace ApiProxies.Commands.Service
{
    public class CancelBrightfieldDustSubtract : ApiCommandBase
    {
        public CancelBrightfieldDustSubtract()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.CancelBrightfieldDustSubtractAPI();
            Log.Debug("CancelBrightfieldDustSubtract:: Result: " + Result);
        }
    }
}
