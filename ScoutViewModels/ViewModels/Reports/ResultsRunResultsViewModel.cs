using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reports;
using ScoutDomains.Reports.Common;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutModels.Reports;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Structs;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReportEventBus = ScoutViewModels.Events.ReportEventBus;

namespace ScoutViewModels.ViewModels.Reports
{
    public class ResultsRunResultsViewModel : BaseViewModel
    {
        #region Constructor

        public ResultsRunResultsViewModel()
        {
            ResultModel = new ResultModel();
            RecordHelper = new ResultRecordHelper();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                DispatcherHelper.ApplicationExecute(() =>
                {
                    ReportPrintTitle = $"{LanguageResourceHelper.Get("LID_Title_ViCellBluVersion")}{UISettings.SoftwareVersion}";
                    FromDate = DateTime.Today;
                    IsAutoExportPDF = true;
                    BarGraphViewList = new List<BarGraphDomain>();
                    DisableAllPrintOptions();
                    LoadPrintOptionsList();
                    ReviewViewModel = new ReviewViewModel();
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        protected override void DisposeUnmanaged()
        {
            foreach (ReportPrintOptions option in ReportPrintOptionsList)
                option.PropertyChanged -= OptionPropertyChanged;
            ReviewViewModel?.Dispose();
            RunResultsReportViewModel?.Dispose();
            RecordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion Constructor

        #region Properties & Fields

        private List<KeyValuePair<int, List<histogrambin_t>>> _histogramList = new List<KeyValuePair<int, List<histogrambin_t>>>();

        public ResultModel ResultModel { get; set; }
        public ResultRecordHelper RecordHelper { get; set; }
        public RunResultsReportViewModel RunResultsReportViewModel { get; private set; }
        public List<BarGraphDomain> GraphListForReport { get; set; }

        public string SampleId
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string UserName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime FromDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<ReportPrintOptions> ReportPrintOptionsList
        {
            get { return GetProperty<ObservableCollection<ReportPrintOptions>>(); }
            set { SetProperty(value); }
        }

        public bool IsGraphChecked
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                IsGraphListBoxVisible = value;
                if (IsGraphListBoxVisible)
                {
                    EnableAndSetGraphOptionsData();
                }
            }
        }

        public string ReportComments
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReportImageCaption
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ReportPrintTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<ReportPrintOptions> ReportGraphParentOptionsList
        {
            get { return GetProperty<ObservableCollection<ReportPrintOptions>>(); }
            set { SetProperty(value);}
        }

        public ReviewViewModel ReviewViewModel
        {
            get { return GetProperty<ReviewViewModel>(); }
            set { SetProperty(value); }
        }

        public bool IsPrintOptionListBoxEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsGraphListBoxVisible
        {
            get { return GetProperty<bool>(); }
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

        public bool IsAutoExportPDF
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<BarGraphDomain> BarGraphViewList
        {
            get { return GetProperty<List<BarGraphDomain>>(); }
            set { SetProperty(value); }
        }

        public SampleRecordDomain SelectedSampleRecordFromList
        {
            get { return GetProperty<SampleRecordDomain>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Graph Selected Command

        private ICommand _isGraphSelectedCommand;
        public ICommand IsGraphSelectedCommand => _isGraphSelectedCommand ?? (_isGraphSelectedCommand = new RelayCommand(IsGraphSelectedExecute));

        private void IsGraphSelectedExecute(object obj)
        {
            IsGraphListBoxVisible = false;
            if (ReportGraphParentOptionsList != null && ReportGraphParentOptionsList.Count > 0 &&
                ReportGraphParentOptionsList[0].IsParameterChecked)
            {
                EnableAndSetGraphOptionsData();
                IsGraphListBoxVisible = true;
            }
        }

        #endregion

        #region Print Command

        private ICommand _printCommand;
        public ICommand PrintCommand => _printCommand ?? (_printCommand = new RelayCommand(PrintReportViewerExecute));

        private async void PrintReportViewerExecute()
        {
            IsAutoExportPDF = false;
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(PrintRunResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                PublishReportProgressIndication(false);
            }
        }

        public void PrintRunResult()
        {
            if (SelectedSampleRecordFromList != null)
            {
                var permission = LoggedInUser.CurrentUserId.Equals(ApplicationConstants.ServiceUser)
                    ? UserPermissionLevel.eService
                    : UserPermissionLevel.eNormal;

                DispatcherHelper.ApplicationExecute(() =>
                {
                    RunResultsReportViewModel = new RunResultsReportViewModel(ReportPrintTitle, ReportComments, permission, SelectedSampleRecordFromList,
                        IsAutoExportPDF, ReportPrintOptionsList, GraphListForReport, GetGraphOptions(), ReportImageCaption);

                    RunResultsReportViewModel.LoadReport();

                    if (!IsAutoExportPDF)
                    {
                        PublishReportProgressIndication(false);
                        ReportEventBus.RunResultsReport(this, RunResultsReportViewModel);
                    }
                });
            }
            else
            {
                PublishReportProgressIndication(false);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Report_NoDataFound"));
            }
        }

        #endregion

        #region Open Sample Command

        private ICommand _openSampleCommand;
        public ICommand OpenSampleCommand => _openSampleCommand ?? (_openSampleCommand = new RelayCommand(OpenSampleExecute));

        private async void OpenSampleExecute()
        {
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(PerformOpenSample);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
            finally
            {
                PublishReportProgressIndication(false);

            }
        }

        private void PerformOpenSample()
        {
            var args = new OpenSampleEventArgs();
            if (DialogEventBus.OpenSampleDialog(this, args) == true && args.SelectedSampleRecord != null)
            {
                var selectedSample = args.SelectedSampleRecord as SampleRecordDomain;
                OnSelectedSample(selectedSample);
            }
            PublishReportProgressIndication(false);
        }

        private void OnSelectedSample(SampleRecordDomain selectedSample)
        {
            try
            {
                IsPrintOptionListBoxEnabled = true;
                RecordHelper.SetImageList(selectedSample);
                if (selectedSample != null)
                {
                    if (selectedSample?.SampleImageList != null && selectedSample.SampleImageList.Any())
                    {
                        var sampleImage = selectedSample.SampleImageList[0];
                        sampleImage.TotalCumulativeImage = Convert.ToInt32(selectedSample.SelectedResultSummary.CumulativeResult.TotalCumulativeImage);
                        var imageData = RecordHelper.GetImage(ImageType.Annotated, sampleImage.BrightFieldId, selectedSample.SelectedResultSummary.UUID);
                        selectedSample.SelectedSampleImageRecord.ImageSet = imageData;
                    }

                    SelectedSampleRecordFromList = selectedSample;

                    if (SelectedSampleRecordFromList != null)
                    {
                        LoadPrintOptionsList();
                        EnablePrintOptions();
                    }

                    if (SelectedSampleRecordFromList != null)
                    {
                        _histogramList = RecordHelper.GetHistogramList(SelectedSampleRecordFromList.SelectedResultSummary.UUID);

                        var graphHelper = new GraphHelper();
                        var graphList = graphHelper.CreateGraph(SelectedSampleRecordFromList, _histogramList, true);
                        if (graphList != null)
                        {
                            GraphListForReport = graphList.ToList();
                        }
                    }

                    if (GraphListForReport != null && GraphListForReport.Count > 0)
                    {
                        EnableAndSetGraphOptionsData();
                    }

                    GraphListForReport?.ForEach(g =>
                    {
                        if (g.SelectedGraphType.Equals(GraphType.AverageSize))
                        {
                            g.YAxisName = LanguageResourceHelper.Get("LID_Label_Size");
                        }

                        g.XAxisName = LanguageResourceHelper.Get("LID_GraphLabel_Image");
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error("Method : OnSelectedSample()  " + ex.Message, ex);
            }
        }

        private void EnablePrintOptions()
        {
            IsExportButtonEnabled = true;
            IsPrintButtonEnabled = true;
            IsPrintOptionListBoxEnabled = true;
            EnableParameterOptions();
        }

        #endregion

        #endregion

        #region Private Methods

        private void LoadPrintOptionsList()
        {
            ReportGraphParentOptionsList = new ObservableCollection<ReportPrintOptions>
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_CheckBox_Graphs"),
                    IsParameterChecked = false
                }
            };
            ReportPrintOptionsList = new ObservableCollection<ReportPrintOptions>()
            {
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_ResultParameters")
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_CheckBox_AnalysisParameters")
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_ReagentParameters")
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_CheckBox_Signatures")
                },
                new ReportPrintOptions()
                {
                    ParameterName = LanguageResourceHelper.Get("LID_Label_FirstAnnotatedImage")
                }
            };
            foreach (ReportPrintOptions option in ReportPrintOptionsList)
                option.PropertyChanged += OptionPropertyChanged;
        }

        private void OptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var printOption = sender as ReportPrintOptions;
            if (printOption == null)
            {
                return;
            }

            if (e.PropertyName.Equals(nameof(printOption.IsParameterChecked)))
            {
                ValidatesSelectedCount();
            }
        }

        private void ValidatesSelectedCount()
        {
            IsPrintButtonEnabled = true;
            var selectedOptionCount = ReportPrintOptionsList.Count(p => p.IsParameterChecked.Equals(true));
            if (selectedOptionCount == 0)
            {
                IsPrintButtonEnabled = false;
            }
        }

        private void EnableParameterOptions()
        {
            var selectionCount = 1;
            const int maxSelectionCount = 4;
            foreach (ReportPrintOptions option in ReportPrintOptionsList)
            {
                if (selectionCount <= maxSelectionCount)
                {
                    option.IsParameterChecked = true;
                }
                option.IsEnabled = true;
                selectionCount++;
            }
        }
        
        private void DisableAllPrintOptions()
        {
            IsExportButtonEnabled = false;
            IsPrintButtonEnabled = false;
            IsPrintOptionListBoxEnabled = false;
            IsGraphListBoxVisible = false;
        }

        private List<ReportGraphOptions> GetGraphOptions()
        {
            var reportGraphOptionsList = new List<ReportGraphOptions>();

            if (GraphListForReport == null) return reportGraphOptionsList;

            for (var index = 0; index < GraphListForReport.Count; index++)
            {
                var graphOptionsObj = new ReportGraphOptions
                {
                    FirstParameterName = GraphListForReport[index].GraphName,
                    IsFirstParameterChecked = true
                };

                if (GraphListForReport.Count > index + 1)
                {
                    graphOptionsObj.SecondParameterName = GraphListForReport[++index].GraphName;
                    graphOptionsObj.IsSecondParameterChecked = true;
                    graphOptionsObj.IsSecondParameterVisible = true;
                }

                reportGraphOptionsList.Add(graphOptionsObj);
            }

            if (GraphListForReport.Count % 2 == 1)
            {
                reportGraphOptionsList[reportGraphOptionsList.Count - 1].IsSecondParameterVisible = false;
            }

            return reportGraphOptionsList;
        }

        private void PublishReportProgressIndication(bool value)
        {
            MessageBus.Default.Publish(new ReportProgressIndicationDomain {
                IsProgressIndicationVisible = value
            });
        }

        private void GraphPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var reportGraphOption = sender as ReportGraphOptions;
            if (reportGraphOption == null)
            {
                return;
            }
            if (e.PropertyName.Equals(nameof(reportGraphOption.IsFirstParameterChecked )) || 
                e.PropertyName.Equals(nameof(reportGraphOption.IsSecondParameterChecked)))
            {
                IsGraphChecked = true;
            }
        }

        #endregion Private Methods

        public void EnableAndSetGraphOptionsData()
        {
            GetGraphOptions();
            foreach (ReportPrintOptions option in ReportGraphParentOptionsList)
                option.IsEnabled = true;
        }
    }
}
