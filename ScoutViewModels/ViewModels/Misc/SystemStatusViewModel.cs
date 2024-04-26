using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutViewModels.ViewModels
{
    public class SystemStatusViewModel : BaseViewModel
    {
        public SystemStatusViewModel(IInstrumentStatusService instrumentStatusService)
        {
            _instrumentStatusService = instrumentStatusService;
            _lastKnownSystemStatus = SystemStatus.Idle;
            Update();
        }

        #region Properties & Fields
        private readonly IInstrumentStatusService _instrumentStatusService;

        private SystemStatus _lastKnownSystemStatus;

        public string SystemStatusString
        {
            get { return GetProperty<string>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public void Update()
        {
            _lastKnownSystemStatus = _instrumentStatusService.SystemStatus;

            DispatcherHelper.ApplicationExecute(() =>
            {
                UpdateSystemStatusString(_lastKnownSystemStatus);
            });
        }

        #endregion

        #region Private Methods

        private void UpdateSystemStatusString(SystemStatus status)
        {
            switch (status)
            {
                case SystemStatus.Idle: SystemStatusString = LanguageResourceHelper.Get("LID_StatusLabel_Idle"); break;
                case SystemStatus.ProcessingSample: SystemStatusString = LanguageResourceHelper.Get("LID_Enum_Running"); break;
                case SystemStatus.Pausing: SystemStatusString = LanguageResourceHelper.Get("LID_MSGBOX_PausingPleaseWait"); break;
                case SystemStatus.Paused: SystemStatusString = LanguageResourceHelper.Get("LID_Enum_Paused"); break;
                case SystemStatus.Stopping: SystemStatusString = LanguageResourceHelper.Get("LID_Enum_Stopping"); break;
                case SystemStatus.Stopped: SystemStatusString = LanguageResourceHelper.Get("LID_StatusLabel_Idle"); break;
                case SystemStatus.Faulted: SystemStatusString = LanguageResourceHelper.Get("LID_StatusLabel_Fault"); break;
                case SystemStatus.SearchingTube: SystemStatusString = LanguageResourceHelper.Get("LID_Enum_SearchingTubes"); break;
                case SystemStatus.NightlyClean: SystemStatusString = LanguageResourceHelper.Get("LID_StatusLabel_NightlyClean"); break;
                default: SystemStatusString = LanguageResourceHelper.Get("LID_Report_Label_Unknown"); break;
            }
        }

        #endregion
    }
}