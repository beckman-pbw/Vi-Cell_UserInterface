namespace ApiProxies.Commands.Service
{
    public class StopContinuousAnalysis : ApiCommandBase
    {
        public StopContinuousAnalysis()
        {
            ManagesMemory = false;
        }

        protected override void InvokeInternal()
        {
            Result = HawkeyeCoreAPI.Service.StopContinuousAnalysisAPI();
        }
    }
}