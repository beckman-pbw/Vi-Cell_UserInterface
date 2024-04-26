namespace ScoutUtilities.Events
{
    public class MessageToken
    {
        public const string MainVm = "MainWindowViewModel";
        public const string AdornerVisible = "IsAdornerVisible";

        public const string RefreshReagentStatus = "RefreshReagentStatus";
        public const string UpdateInactivityTimeout = "UpdateInactivityTimeout";

        public const string SampleSetChanged = "SampleSetChanged";
        public const string CancelSampleSet = "CancelSampleSet";

        public const string SettingsChanged = "SettingsChanged";
        public const string SecuritySettingsChanged = "SecuritySettingsChanged";
        public const string RunSampleSettingsChanged = "RunSampleSettingsChanged";
        public const string UserDefaultCellTypeChanged = "UserDefaultCellTypeChanged";

        public const string RecordsDeleted = "RecordsDeleted";

        public const string MinimizeApplicationWindow = "MinimizeApplicationWindow";

        public const string NewSystemStatus = "NewSystemStatus";

        public const string CellTypesUpdated = "CellTypesUpdated";
        public const string QualityControlsUpdated = "QualityControlsUpdated";

        public const string ShowRunningImagesToggleButtonToken = "ShowRunningImagesToggleButtonToken";

        public const string MainNavMenuOpenCloseToken = "MainNavMenuOpenCloseToken";
    }
}