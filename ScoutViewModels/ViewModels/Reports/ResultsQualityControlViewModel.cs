using HawkeyeCoreAPI;
using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.Reports;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.QualityControl;
using ScoutModels.Reports;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HawkeyeCoreAPI.Facade;
using ScoutServices.Interfaces;
using ScoutUtilities.Helper;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels.Reports
{
    public class ResultsQualityControlViewModel : BaseViewModel
    {
        private readonly IScoutViewModelFactory _viewModelFactory;

        #region Constructor

        public ResultsQualityControlViewModel(IScoutViewModelFactory viewModelFactory) : base()
        {
            _viewModelFactory = viewModelFactory;
            Initialize();
        }


        private void Initialize()
        {
            IsOpenFileEnable = true;

            _resultModel = new ResultModel();
            _recordHelper = new ResultRecordHelper();
            _graphListForReport = new List<LineGraphDomain>();

            SampleRecordList = new List<SampleRecordDomain>();
            GraphList = new List<LineGraphDomain>();

            QualityControlList = CellTypeFacade.Instance.GetAllQualityControls_BECall(out var allCells).ToObservableCollection();
            SelectedQualityControl = QualityControlList.LastOrDefault();
            
            if (SelectedQualityControl != null)
            {
                SampleRecordList = QualityControlModel.RetrieveSampleRecordsForQualityControl(SelectedQualityControl.QcName.Trim());
                IsPrintButtonEnabled = true;
                IsExportButtonEnabled = true;
                IsOpenFileEnable = true;
            }
        }

        protected override void DisposeUnmanaged()
        {
            _recordHelper?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private ResultModel _resultModel;
        private ResultRecordHelper _recordHelper;
        private List<LineGraphDomain> _graphListForReport;

        public bool IsOpenFileEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsExportButtonEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<QualityControlDomain> QualityControlList
        {
            get { return GetProperty<ObservableCollection<QualityControlDomain>>(); }
            set { SetProperty(value); }
        }

        public QualityControlDomain SelectedQualityControl
        {
            get { return GetProperty<QualityControlDomain>(); }
            set
            {
                SetProperty(value);
                SelectSampleCommand.RaiseCanExecuteChanged();
            }
        }
       
        public List<SampleRecordDomain> SampleRecordList
        {
            get { return GetProperty<List<SampleRecordDomain>>(); }
            set { SetProperty(value); }
        }

        public List<LineGraphDomain> GraphList
        {
            get { return GetProperty<List<LineGraphDomain>>(); }
            set { SetProperty(value); }
        }

        //public string PrintTitle
        //{
        //    get { return GetProperty<string>(); }
        //    set { SetProperty(value); }
        //}

        public string Comments
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsPrintButtonEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        #region Export Command

        private RelayCommand _exportCommand;
        public RelayCommand ExportCommand => _exportCommand ?? (_exportCommand = new RelayCommand(PerformExport, null));

        private void PerformExport()
        {
            try
            {
                if (ExportModel.OpenCsvSaveFileDialog(string.Empty, out var fullFilePath))
                {
                    ExportModel.ExportToFile(new List<QualityControlDomain> { SelectedQualityControl }, FileType.Csv, Path.GetDirectoryName(fullFilePath), Path.GetFileName(fullFilePath));
                }
            }
            catch (Exception ex)
            {
                Log.Error("Method : ExportData()  " + ex.Message, ex);
            }
        }

        #endregion

        #region Open Folder Command

        private RelayCommand _openFolderCommand;
        public RelayCommand OpenFolderCommand => _openFolderCommand ?? (_openFolderCommand = new RelayCommand(PerformOpenFolder));

        private void PerformOpenFolder()
        {
            QualityControlList = ScoutModels.LoggedInUser.GetAllowedQcs().ToObservableCollection();
            IsOpenFileEnable = false;
        }

        #endregion

        #region Print Command

        private RelayCommand _printCommand;
        public RelayCommand PrintCommand => _printCommand ?? (_printCommand = new RelayCommand(PerformPrintAsync));

        private async void PerformPrintAsync()
        {
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(OnExecute);
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
                PublishReportProgressIndication(false);
            }
        }

        private void OnExecute()
        {
            OnSelectedSample();
            if (SelectedQualityControl != null)
            {

//TODO: fix this...
                //_resultModel.GetVersionInformation();

                SelectedQualityControl.CellTypeName = "";

                if (ScoutModels.LoggedInUser.IsConsoleUserLoggedIn())
                {
                    SelectedQualityControl.CellTypeName = ScoutModels.LoggedInUser.CurrentUser.AssignedCellTypes
                    .Where(c => c.CellTypeIndex.Equals(SelectedQualityControl.CellTypeIndex))
                    .Select(c => c.CellTypeName).FirstOrDefault();
                }

                DispatcherHelper.ApplicationExecute(() =>
                {
                    var qualityControlsReportViewModel = _viewModelFactory.CreateQualityControlsReportViewModel(ApplicationVersion, Comments, SelectedQualityControl, _graphListForReport);
                    qualityControlsReportViewModel.LoadReport();
                    PublishReportProgressIndication(false);
                    ReportEventBus.QualityControlsReport(this, qualityControlsReportViewModel);
                });
            }
            else
            {
                PublishReportProgressIndication(false);
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Report_NoDataFound"));
            }
        }

        // call this function when you click on print to get the selected sample data
        private void OnSelectedSample()
        {
            try
            {
                var graphHelper = new GraphHelper();
                GraphList = new List<LineGraphDomain>();
                if (SampleRecordList != null && SampleRecordList.Count > 0)
                {
                    foreach (var sample in SampleRecordList)
                    {
                        _recordHelper.OnSelectedSampleRecord(sample);
                    }
                }

                GraphList = graphHelper.CreateLineGraphList(3);
                GraphList = graphHelper.GetLineGraphListForQC(GraphList, SampleRecordList);
                _graphListForReport = GraphList.Where(x => x.GraphDetailList.Count > 0).Select(x => x).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        #endregion

        #region Close Select Sample Command

        private RelayCommand _closeSelectSampleCommand;
        public RelayCommand CloseSelectSampleCommand => _closeSelectSampleCommand ?? (_closeSelectSampleCommand = new RelayCommand(PerformCloseSelectSample));

        public void PerformCloseSelectSample()
        {
            IsOpenFileEnable = true;
        }

        #endregion

        #region Select Sample Command

        private RelayCommand _selectSampleCommand;
        public RelayCommand SelectSampleCommand => _selectSampleCommand ?? (_selectSampleCommand = new RelayCommand(PerformSelectSample, CanPerformSelectSample));

        private bool CanPerformSelectSample()
        {
            return SelectedQualityControl != null;
        }

        private void PerformSelectSample()
        {
            try
            {
                if (SelectedQualityControl == null) return;

                var qcSampleRecordList = _resultModel.RetrieveSampleRecordsForQualityControl(SelectedQualityControl.QcName.Trim());
                SampleRecordList = new List<SampleRecordDomain>();
                
                if (qcSampleRecordList.Count > 0)
                {
                    SampleRecordList = qcSampleRecordList;
                }
                
                if (SelectedQualityControl != null)
                {
                    IsPrintButtonEnabled = true;
                    IsExportButtonEnabled = true;
                }
                
                IsOpenFileEnable = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private void PublishReportProgressIndication(bool value)
        {
            MessageBus.Default.Publish(new ReportProgressIndicationDomain {IsProgressIndicationVisible = value});
        }
        
        #endregion
    }
}