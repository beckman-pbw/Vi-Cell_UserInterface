using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public class SampleResultDialogEventArgs<T> : BaseDialogEventArgs
    {
        public T SampleViewModel { get; set; }
        public SystemStatus SystemState { get; set; }

        public SampleResultDialogEventArgs(T sampleVm, SystemStatus state)
        {
            SizeToContent = false;
            SampleViewModel = sampleVm;
            SystemState = state;
        }
    }
}