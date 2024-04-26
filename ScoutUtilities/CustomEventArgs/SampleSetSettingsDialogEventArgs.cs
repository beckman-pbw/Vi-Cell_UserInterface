namespace ScoutUtilities.CustomEventArgs
{
    public class SampleSetSettingsDialogEventArgs<T> : BaseDialogEventArgs
    {
        public T ColumnSettingViewModelDictionary { get; set; }

        public SampleSetSettingsDialogEventArgs(T dictionary)
        {
            ColumnSettingViewModelDictionary = dictionary;
        }
    }
}