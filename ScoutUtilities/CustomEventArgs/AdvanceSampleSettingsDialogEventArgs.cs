namespace ScoutUtilities.CustomEventArgs
{
    public class AdvanceSampleSettingsDialogEventArgs<T> : BaseDialogEventArgs
    {
        public T AdvancedSampleSettingsViewModel { get; set; }

        public AdvanceSampleSettingsDialogEventArgs(T advancedSampleSettingsViewModel)
        {
            SizeToContent = true;
            AdvancedSampleSettingsViewModel = advancedSampleSettingsViewModel;
        }
    }
}