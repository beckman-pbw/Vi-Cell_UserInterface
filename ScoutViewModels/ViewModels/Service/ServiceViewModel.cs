using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using Ninject;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Service;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutUtilities;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.Service.ConcentrationSlope;

namespace ScoutViewModels.ViewModels.Service
{
    public class ServiceViewModel : BaseViewModel
    {
        public ServiceViewModel(IScoutViewModelFactory viewModelFactory, IAutomationSettingsService automationSettingsService = null)
        {
            _viewModelFactory = viewModelFactory;
            _automationSettingsModel = automationSettingsService ?? new AutomationSettingsService();
            CalibrationSizeContent = new ContentControl();
            ManualControlsCommonContent = new ContentControl();
            OpticsControlsCommonContent = new ContentControl();
            MotorRegContent = new ContentControl();
            CalibrationConcentrationControlsContent = new ContentControl();
            CalibrationModel = new CalibrationModel();
            CalibrationControlItemsViewModel = viewModelFactory.CreateConcentrationViewModel();
            AcupCalibrationControlItemsViewModel = viewModelFactory.CreateAcupConcentrationSlopeViewModel();
            SelectedManualControlIndex = 0;
            SelectedManualOptics = true;
            SelectedStandardCalibration = false;
            SelectedACupCalibration = false;
            SelectedManual = true;
            IsMotorRunning = true;
            IsManualControlsVisible = true;
            ServiceStyle = Application.Current.Resources["ServiceTabControl"] as Style;
            ServiceBottomStyle = Application.Current.Resources["ServiceBottomTabControl"] as Style;
            SelectedCalibrationControlsTabItem = new TabItem();
            CalibrationWidthLeftSide = new GridLength(310);
            CalibrationWidthRightSide = new GridLength(970);

            _boolServiceSubscriber = MessageBus.Default.Subscribe<Notification<bool>>(HandleBoolServiceNotifications);
        }

        protected override void DisposeUnmanaged()
        {
            CalibrationControlItemsViewModel?.Dispose();
            AcupCalibrationControlItemsViewModel?.Dispose();

            ((BaseViewModel)(OpticsControlsCommonContent.Content))?.Dispose();
            ((BaseViewModel)(ManualControlsCommonContent.Content))?.Dispose();
            ((BaseViewModel)(MotorRegContent.Content))?.Dispose();
            MessageBus.Default.UnSubscribe(ref _boolServiceSubscriber);
            base.DisposeUnmanaged();
        }

        #region Event Handlers

        private void HandleBoolServiceNotifications(Notification<bool> msg)
        {
            if (string.IsNullOrEmpty(msg?.Token) || !msg.Token.Equals(GetType().Name) || string.IsNullOrEmpty(msg.Message)) return;

            var newValue = msg.Target;
            var propertyName = msg.Message;

            var propInfo = GetType().GetProperty(propertyName);
            if (propInfo != null && propInfo.GetSetMethod() != null)
            {
                propInfo.SetValue(this, newValue);
            }
        }

        #endregion
        #region Private Properties

        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IAutomationSettingsService _automationSettingsModel;
        private Subscription<Notification<bool>> _boolServiceSubscriber;

        #endregion
        #region Properties

        public MotorRegistrationViewModel MotorRegistrationViewModel { get; set; }

        public bool IsACupEnabled
        {
            get { return CheckIfACupEnabled(); }
            set { SetProperty(value); }
        }

        public bool CheckIfACupEnabled()
        {
            var autoConfig = _automationSettingsModel.GetAutomationConfig();
            return Misc.ByteToBool(autoConfig.ACupIsEnabled);
        }

        public CalibrationModel CalibrationModel
        {
            get { return GetProperty<CalibrationModel>(); }
            set { SetProperty(value); }
        }

        public bool IsSensorStatus
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsMotorRunning
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public ConcentrationViewModel CalibrationControlItemsViewModel
        {
            get { return GetProperty<ConcentrationViewModel>(); }
            set { SetProperty(value); }
        }
        
        public AcupConcentrationSlopeViewModel AcupCalibrationControlItemsViewModel
        {
            get { return GetProperty<AcupConcentrationSlopeViewModel>(); }
            set { SetProperty(value); }
        }

        public ContentControl CalibrationConcentrationControlsContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public GridLength CalibrationWidthLeftSide
        {
            get { return GetProperty<GridLength>(); }
            set { SetProperty(value); }
        }

        public GridLength CalibrationWidthRightSide
        {
            get { return GetProperty<GridLength>(); }
            set { SetProperty(value); }
        }

        public Style ServiceStyle
        {
            get { return GetProperty<Style>(); }
            set { SetProperty(value); }
        }

        public Style ServiceBottomStyle
        {
            get { return GetProperty<Style>(); }
            set { SetProperty(value); }
        }

        public ContentControl CalibrationSizeContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public ContentControl ManualControlsCommonContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public ContentControl OpticsControlsCommonContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public ContentControl MotorRegContent
        {
            get { return GetProperty<ContentControl>(); }
            set { SetProperty(value); }
        }

        public TabItem SelectedCalibrationControlsTabItem
        {
            get { return GetProperty<TabItem>(); }
            set 
            { 
                SetProperty(value);
                SwitchToConcentrationControlsContent();
            }
        }

        public TabItem SelectedManualControlsTabItem
        {
            get { return GetProperty<TabItem>(); }
            set 
            { 
                SetProperty(value);
                if (value != null)
                {
                    SwitchToManualControlsContent(value.Name);
                }
            }
        }

        public int SelectedManualControlIndex
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public bool SelectedManualOptics
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
        
        public bool SelectedStandardCalibration
        {
            get { return GetProperty<bool>(); }
            set 
            { 
                SetProperty(value);
                if (value) SwitchToServiceContent("tbCalibration");
            }
        }

        public bool SelectedACupCalibration
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value) SwitchToServiceContent("tbACupCalibration");
            }
        }

        public bool SelectedManual
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsManualControlsVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Methods
        
        private void SwitchToServiceContent(string name)
        {
            MainWindowViewModel.Instance.SwitchOffLiveImage?.Invoke();
            if (name == string.Empty)
                return;

            switch (name)
            {
                case "tbCalibration":
                    CalibrationWidthLeftSide = new GridLength(310);
                    CalibrationWidthRightSide = new GridLength(970);
                    CalibrationControlItemsViewModel.IsConcentrationTabActive = true;
                    break;

                case "tbManual":
                    CalibrationWidthLeftSide = new GridLength(0);
                    CalibrationWidthRightSide = new GridLength(1280);
                    break;                
            }
        }

        private void SwitchToConcentrationControlsContent()
        {
            CalibrationControlItemsViewModel.ChangesDialogResultCarouselPlate = true;
            CalibrationControlItemsViewModel.ChangesDialogResultCarouselPlate = false;
            CalibrationControlItemsViewModel.SetConcentrationDefaultValue("concentration");
            CalibrationControlItemsViewModel.UpdateConcentrationValue();
            CalibrationControlItemsViewModel.IsConcentrationTabActive = true;
            CalibrationControlItemsViewModel.SetLogEnable(calibration_type.cal_Concentration, 0, 0);
        }

        private void SwitchToManualControlsContent(string param)
        {
            try
            {
                MainWindowViewModel.Instance.SwitchOffLiveImage?.Invoke();
                
                switch (param)
                {
                    case "tbiOptics":
                        ((BaseViewModel)(ManualControlsCommonContent.Content))?.Dispose();
                        ((BaseViewModel)(MotorRegContent.Content))?.Dispose();
                        var optics = _viewModelFactory.CreateManualControlsOpticsViewModel();
                        var firstCellType = CellTypeFacade.Instance.GetAllCellTypes_BECall().First();
                        var result = CellTypeModel.svc_SetTemporaryCellType(firstCellType);
                        var result2 = optics.OpticsManualServiceModel.svc_SetTemporaryAnalysisDefinition(firstCellType.AnalysisDomain);
                        OpticsControlsCommonContent.Content = optics;
                        break;

                    case "tbiLowLevel":
                        ((BaseViewModel)(OpticsControlsCommonContent.Content))?.Dispose();
                        ((BaseViewModel)(MotorRegContent.Content))?.Dispose();
                        ManualControlsCommonContent.Content = _viewModelFactory.CreateLowLevelViewModel();
                        break;

                    case "tbiMotorReg":
                        ((BaseViewModel)(OpticsControlsCommonContent.Content))?.Dispose();
                        ((BaseViewModel)(ManualControlsCommonContent.Content))?.Dispose();
                        MotorRegistrationViewModel = _viewModelFactory.CreateMotorRegistrationViewModel();
                        MotorRegContent.Content = MotorRegistrationViewModel;
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_SWITCH_TO_MANUAL_CONTROLS_CONTENT"));
            }
        }

        #endregion
    }
}
