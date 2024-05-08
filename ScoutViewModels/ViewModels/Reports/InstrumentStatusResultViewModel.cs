using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Reports;
using ScoutDomains.Reports.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Reports;
using ScoutModels.Service;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ScoutModels.Interfaces;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.Reports
{
    public class InstrumentStatusResultViewModel : BaseViewModel
    {
        #region Private Properties

        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IInstrumentStatusService _instrumentStatusService;

        #endregion

        #region Public Properties

        public UserModel UserModel { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand OnInstrumentTypeSelectionChangedCommand { get; set; }
        public List<UserDomain> UserList { get; set; }
        public List<CellTypeDomain> AllCellTypesList { get; set; }
        public List<CalibrationActivityLogDomain> ConcDataListList { get; set; }
        public List<CalibrationActivityLogDomain> ACupConcDataListList { get; set; }
        public string AnalysisType { get; set; }
        public ReagentModel ReagentModel { get; set; }
        public IList<ReagentContainerStateDomain> ReagentContainers { get; private set; }

        public List<ReportPrintOptions> InstrumentStatusPrintOptionsList
        {
            get { return GetProperty<List<ReportPrintOptions>>(); }
            set { SetProperty(value); }
        }

        public bool IsPrintButtonEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExportButtonEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string PrintTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Comment
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsACupEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion Public Properties

        #region Private Properties

        private IDisposable _statusSubscriber;

        #endregion

        #region Constructor

        public ResultModel ResultModel;

        public InstrumentStatusResultViewModel(IScoutViewModelFactory viewModelFactory, IInstrumentStatusService instrumentStatusService,
            IAutomationSettingsService automationSettingsService) : base()
        {
            _viewModelFactory = viewModelFactory;
            _instrumentStatusService = instrumentStatusService;
            var autoConfig = automationSettingsService.GetAutomationConfig();
            IsACupEnabled = Misc.ByteToBool(autoConfig.ACupIsEnabled);
            ResultModel = new ResultModel();
            UserModel = new UserModel();
            Initialize();
        }

        public SystemStatusDomain SystemStatusDomain;
        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            if (systemStatusDomain == null) return;
            
            SystemStatusDomain = systemStatusDomain;
        }

        private void Initialize()
        {
            IsPrintButtonEnabled = false;
            IsExportButtonEnabled = false;
            OnInstrumentTypeSelectionChangedCommand = new RelayCommand(OnInstrumentTypeSelectionChangedExecute, null);
            PrintCommand = new RelayCommand(PrintExecute, null);
            LoadParameterList();
            PrintTitle = $"{LanguageResourceHelper.Get("LID_Title_ViCellBluVersion")}{UISettings.SoftwareVersion}";
            _statusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
        }

        protected override void DisposeUnmanaged()
        {
            _statusSubscriber?.Dispose();
            ReagentModel?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion Constructor

        #region Private Methods

        private void OnInstrumentTypeSelectionChangedExecute()
        {
            IsPrintButtonEnabled = false;
            IsExportButtonEnabled = false;
            var isAnyItemChecked = InstrumentStatusPrintOptionsList.Any(x => x.IsParameterChecked == true);
            if (!isAnyItemChecked)
            {
                return;
            }
            IsPrintButtonEnabled = true;
            IsExportButtonEnabled = true;
        }

        private async void PrintExecute(object obj)
        {
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(() => { OnExecute(); });
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ADMIN_BIOPROCESS_PRINT"));
                PublishReportProgressIndication(false);
            }
        }

     
        private void OnExecute()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                GetDataForPrint();
                var instrumentStatusReportViewModel = _viewModelFactory.CreateInstrumentStatusReportViewModel(PrintTitle, Comment, SystemStatusDomain,
                    InstrumentStatusPrintOptionsList, ConcDataListList, ACupConcDataListList, UserList, ReagentContainers, AnalysisType);
                instrumentStatusReportViewModel.LoadReport();
                PublishReportProgressIndication(false);
                ReportEventBus.InstrumentStatusReport(this, instrumentStatusReportViewModel);
            });
            //}
            //else
            //{
            //    PublishReportProgressIndication(false);
            //    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Report_NoDataFound"));
            //}
        }

        private void PublishReportProgressIndication(bool value)
        {
            MessageBus.Default.Publish(new ReportProgressIndicationDomain { 
                IsProgressIndicationVisible = value 
            });
        }

        private void LoadParameterList()
        {
            InstrumentStatusPrintOptionsList = new List<ReportPrintOptions>()
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_ResultHeader_Reagents"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.Reagents
                },                
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_GridLabel_CellTypes"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.CellType
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_AppType"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.AnalysisType
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_TabItem_CalibrationControl"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.ConcentrationSlope
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_TabItem_ACupConcSlope"),
                    IsParameterChecked = IsACupEnabled,
                    InstrumentoptionType = InstrumentStatusOptionType.ACupConcentrationSlope
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_FrameLabel_Storage"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.Storage
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_TabItem_LowLevel"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.LowLevel
                }
            };
            if(!LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eNormal))
            {
                InstrumentStatusPrintOptionsList.Add(new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_TabItem_Users"),
                    IsParameterChecked = true,
                    InstrumentoptionType = InstrumentStatusOptionType.Users
                });
            }
        }

        private void GetDataForPrint()
        {
            foreach (var status in InstrumentStatusPrintOptionsList)
            {
                switch (status.InstrumentoptionType)
                {
                    case InstrumentStatusOptionType.Reagents:
                        if (status.IsParameterChecked)
                        {
                            if (ReagentModel == null)
                            {
                                ReagentModel = new ReagentModel();
                            }
                            ReagentContainers = ReagentModel.GetReagentContainerStatusAll();
                        }
                        break;

                    case InstrumentStatusOptionType.Users:
                        if (status.IsParameterChecked)
                        {
                            UserList = UserModel.GetUserList();
                        }
                        break;

                    case InstrumentStatusOptionType.CellType:
                        if (status.IsParameterChecked)
                        {
                            if (LoggedInUser.IsConsoleUserLoggedIn())
                            {
                                AllCellTypesList = LoggedInUser.CurrentUser.AssignedCellTypes;
                            }
                            else
                            {
                                AllCellTypesList = new List<CellTypeDomain>();
                            }                            
                        }
                        break;

                    case InstrumentStatusOptionType.AnalysisType:
                        if (status.IsParameterChecked)                          
                        {
                            var analysisTypes = AnalysisModel.GetAllAnalyses();
                            AnalysisType = analysisTypes?.FirstOrDefault()?.Label ?? string.Empty;
                        }
                        break;

                    case InstrumentStatusOptionType.ConcentrationSlope:
                        if (status.IsParameterChecked)
                        {
                            ConcDataListList = CalibrationModel.RetrieveCalibrationActivityLog(calibration_type.cal_Concentration);
                        }
                        break;
                    case InstrumentStatusOptionType.ACupConcentrationSlope:
                        if (status.IsParameterChecked)
                        {
                            ACupConcDataListList = CalibrationModel.RetrieveCalibrationActivityLog(calibration_type.cal_ACupConcentration);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        
        #endregion Private Methods

    }
}
