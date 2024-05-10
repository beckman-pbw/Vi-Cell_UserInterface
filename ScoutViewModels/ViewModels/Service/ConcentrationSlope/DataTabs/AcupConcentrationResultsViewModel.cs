using ScoutDomains;
using ScoutDomains.Common;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs
{
    public class AcupConcentrationResultsViewModel : BaseViewModel, IHandlesCalibrationState, IHandlesSampleStatus,
        IHandlesImageReceived, IHandlesConcentrationCalculated
    {
        public AcupConcentrationResultsViewModel(IAcupConcentrationService acupConcentrationService,
            IConcentrationSlopeService concentrationSlopeService)
        {
            _acupConcentrationService = acupConcentrationService;
            _concentrationSlopeService = concentrationSlopeService;
            IsSingleton = true;
            HandleNewCalibrationState(CalibrationGuiState.NotStarted);
            ResetColumnHeaders();
        }

        #region Properties & Fields

        private readonly IAcupConcentrationService _acupConcentrationService;
        private readonly IConcentrationSlopeService _concentrationSlopeService;

        public bool IsCalibrationCompleted
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsConcentrationUpdateActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ConcentrationIsCalculated
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double CalibrationSlope
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double CalibrationIntercept
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double CalibrationR2
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<DataGridExpanderColumnHeader> ColumnHeaders
        {
            get { return GetProperty<ObservableCollection<DataGridExpanderColumnHeader>>(); }
            set { SetProperty(value); }
        }

        public DataGridExpanderColumnHeader SelectedColumnHeader
        {
            get { return GetProperty<DataGridExpanderColumnHeader>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Interface Methods

        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            switch(state)
            {
                case CalibrationGuiState.CalibrationApplied:
                case CalibrationGuiState.CalibrationRejected:
                case CalibrationGuiState.Aborted:
                case CalibrationGuiState.NotStarted:
                    ColumnHeaders = new ObservableCollection<DataGridExpanderColumnHeader>();
                    goto case CalibrationGuiState.Started;
                    
                case CalibrationGuiState.Started:
                    IsCalibrationCompleted = false;
                    ConcentrationIsCalculated = false;
                    IsConcentrationUpdateActive = false;
                    CalibrationSlope = 0d;
                    CalibrationIntercept = 0d;
                    CalibrationR2 = 0d;
                    break;

                case CalibrationGuiState.Ended:
                    IsCalibrationCompleted = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        public void HandleSampleStatusChanged(SampleEswDomain sample, AcupCalibrationConcentrationListDomain concentration)
        {
            AddToColumnHeaders(sample, concentration);
        }
        
        public void HandleImageReceived(SampleEswDomain sample, ushort imageNum, BasicResultAnswers cumulativeResults,
            ImageSetDto image, BasicResultAnswers imageResults, AcupCalibrationConcentrationListDomain concentration)
        {
            var preDataGridColumn = ColumnHeaders.FirstOrDefault(x => x.SampleId == sample.SampleName);
            if (preDataGridColumn == null)
            {
                AddToColumnHeaders(sample, concentration);
            }
            else
            {
                preDataGridColumn.Original = cumulativeResults.concentration_general;
                preDataGridColumn.TotCount = cumulativeResults.count_pop_general;
                SetAverageOriginalValues();
            }
        }
        
        public void HandleConcentrationCalculated(
            CalibrationData totalCells,
            CalibrationData originalConcentration,
            CalibrationData adjustedConcentration)
        {
            // Update the Adjusted Concentration in the DataGrid
            if (adjustedConcentration.Data.Count <= 0 || ColumnHeaders.Count != adjustedConcentration.Data.Count)
                return;

            for (var i = 0; i < ColumnHeaders.Count; i++)
            {
                var index = i;
                DispatcherHelper.ApplicationExecute(() =>
                {
                    ColumnHeaders[index].Adjusted = adjustedConcentration.Data[index].Value;
                });
            }

            // Calculate the CV
            var data = new CalibrationData();
            var headerList = ColumnHeaders.GroupBy(x => x.AssayValue);
            foreach (var assayGroup in headerList)
            {
                var cellCount = new List<double>();
                foreach (var columnHeader in assayGroup)
                {
                    cellCount.Add(columnHeader.TotCount);
                }
                var cv = data.CalculateCV(cellCount, assayGroup.ElementAt(0).AvgTotCount);
                DispatcherHelper.ApplicationExecute(() =>
                {
                    assayGroup.ElementAt(0).PercentCV = Misc.UpdateTrailingPoint(cv, TrailingPoint.One);
                });
            }

            // Update the Average Adjusted Concentration
            // NOTE: There is code that lives in the ExpanderInDataGrid.xaml.cs file that also does this.
            // It doesn't seem to update the average for a cup calibration, though. So I am adding it here.
            _concentrationSlopeService.UpdateAvgAdjustedValues(ColumnHeaders);

            // Update the slope results
            DispatcherHelper.ApplicationExecute(() =>
            {
                CalibrationSlope = totalCells.Slope;
                CalibrationR2 = totalCells.R2;
                CalibrationIntercept = totalCells.Intercept;
                NotifyPropertyChanged(nameof(ColumnHeaders));
                NotifyPropertyChanged(nameof(SelectedColumnHeader));
            });
        }

        #endregion

        #region Private Helper Methods

        private void ResetColumnHeaders()
        {
            ColumnHeaders = new ObservableCollection<DataGridExpanderColumnHeader>();
            var concentrationList = _concentrationSlopeService.GetStandardConcentrationList();
            foreach (var concentration in concentrationList)
            {
                var pos = 1;
                switch (concentration.AssayValueType)
                {
                    case AssayValueEnum.M2:
                        pos = ApplicationConstants.StartPosition2M + 1; break;
                    case AssayValueEnum.M4:
                        pos = ApplicationConstants.StartPosition4M + 1; break;
                    case AssayValueEnum.M10:
                        pos = ApplicationConstants.StartPosition10M + 1; break;
                }

                var columnHeader = new DataGridExpanderColumnHeader
                {
                    SampleId = Misc.GetConcentrationSampleName(concentration.AssayValueType, pos),
                    AssayValue = _concentrationSlopeService.ConvertAssayValueToDouble(concentration),
                    Original = double.NaN,
                    Adjusted = double.NaN
                };

                ColumnHeaders.Add(columnHeader);
            }
        }

        private void AddToColumnHeaders(SampleEswDomain sample, AcupCalibrationConcentrationListDomain concentration)
        {
            var preDataGridColumn = ColumnHeaders.FirstOrDefault(x => x.SampleId == sample.SampleName);
            if (preDataGridColumn == null && concentration != null)
            {
                preDataGridColumn = new DataGridExpanderColumnHeader();
                preDataGridColumn.SampleId = sample.SampleName;
                preDataGridColumn.DilutionFactor = (int)sample.Dilution;
                preDataGridColumn.AssayValue = _concentrationSlopeService.ConvertAssayValueToDouble(concentration);
                preDataGridColumn.ExpirationDate = concentration.ExpiryDate;
                preDataGridColumn.KnownConcentration = concentration.KnownConcentration;
                preDataGridColumn.Lot = concentration.Lot;
                preDataGridColumn.Original = double.NaN;
                preDataGridColumn.Adjusted = double.NaN;
                preDataGridColumn.AvgAdjusted = double.NaN;
                preDataGridColumn.AvgOriginal = double.NaN;

                DispatcherHelper.ApplicationExecute(() => { ColumnHeaders.Add(preDataGridColumn); });
            }
        }

        private void SetAverageOriginalValues()
        {
            _concentrationSlopeService.UpdateAverageOriginalValues(ColumnHeaders);
            DispatcherHelper.ApplicationExecute(() => NotifyPropertyChanged(nameof(ColumnHeaders)));
        }

        #endregion

        #region Commands

        #region On Loaded

        private RelayCommand _onLoaded;
        public RelayCommand OnLoaded => _onLoaded ??(_onLoaded = new RelayCommand(PerformLoaded));

        private void PerformLoaded()
        {
            // The UI Control for ExpanderInDataGrid is wonky and includes an extra set of rows when
            // first drawn. Setting this variable to another value forces a redraw which corrects it.
            // There are 2 of them so that the original value is preserved.
            IsConcentrationUpdateActive = !IsConcentrationUpdateActive;
            IsConcentrationUpdateActive = !IsConcentrationUpdateActive;

            // The following is code that can be used for easier debugging.
            // This code will set the ExpanderDataGrid values based on some fake data.
            //ColumnHeaders = new ObservableCollection<DataGridExpanderColumnHeader>();
            //ColumnHeaders = GetFakeColumnHeaders().ToObservableCollection();
            
            //SetAverageOriginalValues();
            //_concentrationSlopeService.UpdateAvgAdjustedValues(ColumnHeaders);
			
            //CalcFakeData(out var totalCells, out var ogConcentration, out var adjConcentration);
			//HandleConcentrationCalculated(totalCells, ogConcentration, adjConcentration);
            //HandleNewCalibrationState(CalibrationGuiState.Ended);
            // End Debug Code
        }

        #endregion

        #endregion

        #region Debugging Methods

        private void GetFakeData(out CalibrationData totalCells,
            out CalibrationData originalConcentration,
            out CalibrationData adjustedConcentration)
        {
            totalCells = JsonConvert.DeserializeObject<CalibrationData>(
                @"{'Data':[
{'Key':2000000.0,'Value':576.0},
{'Key':2000000.0,'Value':401.0},
{'Key':4000000.0,'Value':576.0},
{'Key':4000000.0,'Value':489.0},
{'Key':10000000.0,'Value':576.0},
{'Key':10000000.0,'Value':607.0}
],
'Slope':0.00001217307692,'Intercept':472.5769231,'R2':0.3395}");
            originalConcentration = JsonConvert.DeserializeObject<CalibrationData>(
                @"{'Data':[
{'Key':2000000.0,'Value':3472222},
{'Key':2000000.0,'Value':4987531},
{'Key':4000000.0,'Value':6944444},
{'Key':4000000.0,'Value':8179959},
{'Key':10000000.0,'Value':17361110},
{'Key':10000000.0,'Value':16474460}
],'Slope':1.579821337,'Intercept':1144240.539,'R2':0.9867}");
            adjustedConcentration = JsonConvert.DeserializeObject<CalibrationData>(
                @"{'Data':[
{'Key':2000000.0,'Value':2000000.0},
{'Key':2000000.0,'Value':2000000.0},
{'Key':4000000.0,'Value':4000000.0},
{'Key':4000000.0,'Value':4000000.0},
{'Key':10000000.0,'Value':10000000.0},
{'Key':10000000.0,'Value':10000000.0}
],'Slope':1.0,'Intercept':0.0,'R2':1.0}");
        }

        private IList<DataGridExpanderColumnHeader> GetFakeColumnHeaders()
        {
            var str = @"[
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.001',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': '0.1',
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.002',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.003',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5341138.8256913819,
		'AssayValue': 2000000.0,
		'TotCount': 576.0,
		'Original': 211968.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.004',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5322593.204768841,
		'AssayValue': 2000000.0,
		'TotCount': 574.0,
		'Original': 211232.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.005',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.006',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.007',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5322593.204768841,
		'AssayValue': 2000000.0,
		'TotCount': 574.0,
		'Original': 211232.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.008',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5341138.8256913819,
		'AssayValue': 2000000.0,
		'TotCount': 576.0,
		'Original': 211968.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.009',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_2M.010',
		'KnownSize': null,
		'KnownConcentration': '2 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 2000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211600.0,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_4M.011',
		'KnownSize': null,
		'KnownConcentration': '4 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': '0.2',
		'ImageId': 0,
		'Adjusted': 5341138.8256913819,
		'AssayValue': 4000000.0,
		'TotCount': 576.0,
		'Original': 211968.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211526.4,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_4M.012',
		'KnownSize': null,
		'KnownConcentration': '4 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5322593.204768841,
		'AssayValue': 4000000.0,
		'TotCount': 574.0,
		'Original': 211232.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211526.4,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_4M.013',
		'KnownSize': null,
		'KnownConcentration': '4 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 4000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211526.4,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_4M.014',
		'KnownSize': null,
		'KnownConcentration': '4 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 4000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211526.4,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_4M.015',
		'KnownSize': null,
		'KnownConcentration': '4 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5322593.204768841,
		'AssayValue': 4000000.0,
		'TotCount': 574.0,
		'Original': 211232.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211526.4,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_10M.016',
		'KnownSize': null,
		'KnownConcentration': '10 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': '0.1',
		'ImageId': 0,
		'Adjusted': 5341138.8256913819,
		'AssayValue': 10000000.0,
		'TotCount': 576.0,
		'Original': 211968.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211722.67,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_10M.017',
		'KnownSize': null,
		'KnownConcentration': '10 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 10000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211722.67,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	},
	{
		'DilutionFactor': 1,
		'SampleId': 'ConcSlope_10M.018',
		'KnownSize': null,
		'KnownConcentration': '10 M',
		'Lot': 'a',
		'ExpirationDate': '2021-04-16T09:11:09.2286181-07:00',
		'PercentCV': null,
		'ImageId': 0,
		'Adjusted': 5331866.015230112,
		'AssayValue': 10000000.0,
		'TotCount': 575.0,
		'Original': 211600.0,
		'Validate': false,
		'AvgTotCount': 575.0,
		'AvgOriginal': 211722.67,
		'AvgAdjusted': 'NaN',
		'AvgValidate': false,
		'IsStatusUpdated': false
	}
]";
            return JsonConvert.DeserializeObject<IList<DataGridExpanderColumnHeader>>(str);
        }

        private void CalcFakeData(out CalibrationData totalCells,
            out CalibrationData ogCalibration,
            out CalibrationData adjCalibration)
        {
            var columnHeaders = GetFakeColumnHeaders();
            totalCells = new CalibrationData();
            ogCalibration = new CalibrationData();
            adjCalibration = new CalibrationData();

            foreach (var column in columnHeaders)
            {
                var assayValue = column.AssayValue;
                var adj = column.Adjusted;
				totalCells.Data.Add(new KeyValuePair<double, double>(assayValue, adj));
				ogCalibration.Data.Add(new KeyValuePair<double, double>(assayValue, column.Original));
            }

            totalCells.CalculateSlope_averageOverAssays();
            ogCalibration.CalculateSlope_averageOverAssays();
			
            foreach (var pair in totalCells.Data)
            {
                var adjConcValue = (pair.Value * totalCells.Slope) + totalCells.Intercept;
                adjCalibration.Data.Add(new KeyValuePair<double, double>(pair.Key, adjConcValue));
            }
            
            adjCalibration.CalculateSlope_averageOverAssays();
		}

        #endregion
    }
}