using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Reports;
using ScoutDomains.Reports.Common;
using ScoutDomains.RunResult;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Reports;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace ScoutViewModels.ViewModels.Reports
{
    public class CompletedRunSummaryResultViewModel : BaseViewModel
    {
        #region Public Properties

        public ICommand PrintCommand { get; set; }
        public ResultRecordHelper RecordHelper { get; set; }
        public ResultModel ResultModel;
        public UserModel UserModel;
        public List<ReportPrintOptions> DefaultParameterList;

        public List<ReportPrintOptions> ReportPrintOptionsList
        {
            get { return GetProperty<List<ReportPrintOptions>>(); }
            set { SetProperty(value); }
        }

        public List<ReportPrintOptions> AnalysisParameterList
        {
            get { return GetProperty<List<ReportPrintOptions>>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.CompletedRuns_Comments)]
        public string Comments
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.CompletedRuns_SelectedUser)]
        public UserDomain SelectedUser
        {
            get { return GetProperty<UserDomain>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.CompletedRuns_FromDate)]
        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        [SessionVariable(SessionKey.CompletedRuns_ToDate)]
        public DateTime ToDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public IList<UserDomain> UserList
        {
            get { return GetProperty<IList<UserDomain>>(); }
            set { SetProperty(value); }
        }

        public List<SampleRecordDomain> SampleRecordListForPrint
        {
            get { return GetProperty<List<SampleRecordDomain>>(); }
            set { SetProperty(value); }
        }

        #endregion Public Properties

        #region Constructor

        public CompletedRunSummaryResultViewModel() : base()
        {
            ResultModel = new ResultModel();
            RecordHelper = new ResultRecordHelper();
            UserModel = new UserModel();
            Initialize();
        }

        public CompletedRunSummaryResultViewModel(ResultModel resultModel, UserModel userModel, MainWindowViewModel mainWindowVm, 
            ResultRecordHelper recordHelper) : base()
        {
            ResultModel = resultModel;
            RecordHelper = recordHelper;
            UserModel = userModel;
            Initialize();
        }

        private void Initialize()
        {
            PrintCommand = new RelayCommand(PrintExecute, null);
            UserList = new List<UserDomain>(UserModel.UserList);
            SampleRecordListForPrint = new List<SampleRecordDomain>();

            LoadList();
            LoadAnalysisParameters();
            LoadDefaultParameters();
            SetUserList();

            var currentUserSession = LoggedInUser.CurrentUser.Session;
            Comments = currentUserSession.GetVariable(SessionKey.CompletedRuns_Comments, string.Empty);

            FromDate = DateTime.Today.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract);
            ToDate = DateTime.Now;
        }

        protected override void DisposeUnmanaged()
        {
            RecordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion Constructor

        private async void PrintExecute()
        {
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(() => { OnExecute(); });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                PublishReportProgressIndication(false);
            }
        }

        private void OnExecute()
        {
            var fromDateGreaterThanToDate = GetAllSampleData();
            if (SampleRecordListForPrint != null && SampleRecordListForPrint.Count > 0 && fromDateGreaterThanToDate)
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    var runSummaryReportViewModel = new RunSummaryReportViewModel(ApplicationVersion, Comments, ReportPrintOptionsList, AnalysisParameterList, 
                        DefaultParameterList, SampleRecordListForPrint);
                    runSummaryReportViewModel.LoadReport();
                    PublishReportProgressIndication(false);
                    ReportEventBus.RunSummaryReport(this, runSummaryReportViewModel);
                });
                
            }
            else if (fromDateGreaterThanToDate)
            {
                PublishReportProgressIndication(false);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Report_NoDataFound"));
            }
            else
            {
                PublishReportProgressIndication(false);
            }

        }

        private void PublishReportProgressIndication(bool value)
        {
             MessageBus.Default.Publish(new ReportProgressIndicationDomain { 
                 IsProgressIndicationVisible = value 
             });
        }

        private void LoadList()
        {
            ReportPrintOptionsList = new List<ReportPrintOptions>()
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Result_Totalcells"),                    
                    ParameterType = RunSummaryParameterType.TotalCells,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_RunResultsReport_Graph_ViableCells_Title"),                    
                    ParameterType = RunSummaryParameterType.ViableCells,
                    AlignmentType = HorizontalAlignmentType.Right
                },                
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_Size"),
                    ParameterType = RunSummaryParameterType.Size,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_GraphLabel_Viablesize"),
                    ParameterType = RunSummaryParameterType.ViableSize,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_AverageCircularity"),
                    ParameterType = RunSummaryParameterType.Circularity,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_GraphLabel_Viablecircularity"),
                    ParameterType = RunSummaryParameterType.ViableCircularity,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Result_AverageCellPerImage"),
                    ParameterType = RunSummaryParameterType.AverageCellsPerImage,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_GraphLabel_Averagebackground"),
                    ParameterType = RunSummaryParameterType.AverageBrightField,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_bubbles"),
                    ParameterType = RunSummaryParameterType.Bubbles,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_GraphLabel_CellClusters"),
                    ParameterType = RunSummaryParameterType.ClusterCount,
                    AlignmentType = HorizontalAlignmentType.Right
                }
            };
            ReportPrintOptionsList.ForEach(r => r.PropertyChanged += ResultPropertyChanged);
        }

        private void LoadAnalysisParameters()
        {
            AnalysisParameterList = new List<ReportPrintOptions>()
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Icon_CellType"),
                    IsParameterChecked = true,
                    IsEnabled = true,
                    ParameterType = RunSummaryParameterType.CellType,
                    AlignmentType = HorizontalAlignmentType.Left
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_QCHeader_ReAnalysisDate"),
                    ParameterType = RunSummaryParameterType.ReAnalysisDate,
                    AlignmentType = HorizontalAlignmentType.Left
                },

                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Report_AnalysisBy"),
                    IsParameterChecked = true,
                    IsEnabled = true,ParameterType = RunSummaryParameterType.AnalysisBy,
                    AlignmentType = HorizontalAlignmentType.Left
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Report_ReanalysisBy"),
                    ParameterType = RunSummaryParameterType.ReAnalysisBy,
                    AlignmentType = HorizontalAlignmentType.Left
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_Dilution"),
                    ParameterType = RunSummaryParameterType.Dilution,
                    AlignmentType = HorizontalAlignmentType.Right
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_Tag"),
                    ParameterType = RunSummaryParameterType.Tag,
                    AlignmentType = HorizontalAlignmentType.Left
                },
                  new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Icon_Signature"),
                    IsParameterChecked = true,
                    IsEnabled = true,
                    ParameterType = RunSummaryParameterType.Signature,
                    AlignmentType = HorizontalAlignmentType.Left
                }
            };
            AnalysisParameterList.ForEach(a => a. PropertyChanged += AnalysisPropertyChanged);
        }

    
        private void LoadDefaultParameters()
        {
            DefaultParameterList = new List<ReportPrintOptions>()
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_QCHeader_AnalysisDate"),
                    ParameterType = RunSummaryParameterType.AnalysisDate,
                    ColumnType = RunSummaryColumnType.ColumnOne
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_ReportLabel_Concentration_WithoutUnit"),
                    ParameterType = RunSummaryParameterType.Concentration,
                    ColumnType = RunSummaryColumnType.ColumnTwo
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_ViableConcentrationWithoutUnit"),
                    ParameterType = RunSummaryParameterType.ViableConcentration,
                    ColumnType = RunSummaryColumnType.ColumnThree
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_TabItem_Viability"),
                    ParameterType = RunSummaryParameterType.Viability,
                    ColumnType = RunSummaryColumnType.ColumnFour
                }
            };
        }
    
        private void ResultPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var reportPrintOption = sender as ReportPrintOptions;
            if (reportPrintOption == null)
            {
                return;
            }
            if (e.PropertyName.Equals(nameof(reportPrintOption.IsParameterChecked)))
            {
                ValidateSelectedTotalCount();
            }
        }

      
        private void AnalysisPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var reportPrintOption = sender as ReportPrintOptions;
            if (reportPrintOption == null)
            {
                return;
            }
            if (e.PropertyName.Equals(nameof(reportPrintOption.IsParameterChecked)))
            {
                ValidateSelectedTotalCount();
            }
        }
    
        private void ValidateSelectedTotalCount()
        {
            const int maximumSelectionCount = 3;
            var resultParameterSelectedCount =
                ReportPrintOptionsList.Count(r => r.IsParameterChecked.Equals(true));
            var analysisParameterSelectedCount =
                AnalysisParameterList.Count(r => r.IsParameterChecked.Equals(true));
            var selectedCount = resultParameterSelectedCount + analysisParameterSelectedCount;
            if (selectedCount.Equals(maximumSelectionCount))
            {
                foreach (ReportPrintOptions option in ReportPrintOptionsList.Where(r => !r.IsParameterChecked))
                        option.IsEnabled = false;

                foreach (ReportPrintOptions option in AnalysisParameterList.Where(r => !r.IsParameterChecked))
                        option.IsEnabled = false;

            }
            else
            {
                ReportPrintOptionsList.ForEach(r => r.IsEnabled = true);
                AnalysisParameterList.ForEach(r => r.IsEnabled = true);
            } 
        }


        private void SetUserList()
        {
            UserList.Add(new UserDomain { UserID = LanguageResourceHelper.Get("LID_Label_All") });

            UserDomain user;
            switch (LoggedInUser.CurrentUserId)
            {
                case ApplicationConstants.ServiceUser:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceUser });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceUser));
                    break;
                case ApplicationConstants.SilentAdmin:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.SilentAdmin });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.SilentAdmin));
                    break;
                case ApplicationConstants.AutomationClient:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.AutomationClient });
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.AutomationClient));
                    break;
                //// Leave for future reference or use
                //case ApplicationConstants.ServiceAdmin:
                //    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceAdmin });
                //    user = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceAdmin));
                //    break;
                default:
                    user = UserList.FirstOrDefault(a => a.UserID.Equals(LoggedInUser.CurrentUserId));
                    break;
            }

            if (user == null) user = UserList.FirstOrDefault();
            user = SavedSession.GetVariable(SessionKey.CompletedRuns_SelectedUser, user);
            SelectedUser = UserList.FirstOrDefault(u => u.UserID.Equals(user.UserID));
        }


        private bool GetAllSampleData()
        {
            if (!Validation.OnDateValidate(FromDate, ToDate))
                return false;
            if (SelectedUser.UserID.Equals(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_All")))
            {
                SampleRecordListForPrint =  GetSampleDataForUser(string.Empty).ToList();
            }
            else
            {
                SampleRecordListForPrint = GetSampleDataForUser(SelectedUser.UserID);
            }

            return true;

        }

        private List<SampleRecordDomain> GetSampleDataForUser(string userId)
        {
            var sampleRecordList = ResultModel.RetrieveSampleRecords(DateTimeConversionHelper.DateTimeToUnixSecondRounded(FromDate),
                DateTimeConversionHelper.DateTimeToUnixSecondRounded(ToDate.Date.AddDays(1)), userId);

            //To get the result record signature list
            if (sampleRecordList != null&& sampleRecordList.Any())
            {
                sampleRecordList.ForEach(sampleRec =>
                {
                    foreach (ResultSummaryDomain resultSum in sampleRec.ResultSummaryList)
                    {
                        var uuid = resultSum?.UUID;
                        var resultRecord = uuid.HasValue ? sampleRec.GetResultRecord(uuid.Value) : null;
                        if (resultRecord?.ResultSummary != null)
                        {
                            resultSum.SignatureList = resultRecord.ResultSummary.SignatureList;
                            resultSum.SelectedSignature = resultRecord.ResultSummary.SelectedSignature;
                        }
                    }
                });
            }
            return sampleRecordList;
        }

    }

}
