using Newtonsoft.Json;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutServices.Service.ConcentrationSlope;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope.DataTabs
{
    public class AcupGraphsTabViewModel : BaseViewModel, IHandlesCalibrationState, IHandlesConcentrationCalculated
    {
        public AcupGraphsTabViewModel(IConcentrationSlopeService concentrationService)
        {
            _concentrationService = concentrationService;
            IsSingleton = true;
            ResetGraphList();
        }

        #region Properties & Fields

        private readonly IConcentrationSlopeService _concentrationService;
        private LineGraphDomain _selectedGraph;
        
        public ObservableCollection<LineGraphDomain> GraphViewList
        {
            get { return GetProperty<ObservableCollection<LineGraphDomain>>(); }
            set { SetProperty(value); }
        }
        
        public int SelectedGraphIndex
        {
            get { return GetProperty<int>(); }
            set
            {
                if (value < GraphViewList.Count)
                {
                    SetProperty(value);
                    if (value >= 0) _selectedGraph = GraphViewList[value];
                    else _selectedGraph = new LineGraphDomain();
                }
            }
        }

        public bool CalibrationFinished
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                GraphExpandCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Interface Methods

        public void HandleNewCalibrationState(CalibrationGuiState state)
        {
            switch(state)
            {
                case CalibrationGuiState.NotStarted:
                case CalibrationGuiState.Started:
                case CalibrationGuiState.CalibrationApplied:
                case CalibrationGuiState.CalibrationRejected:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        CalibrationFinished = false;
                        ResetGraphList();
                    });
                    break;
                case CalibrationGuiState.Aborted:
                    break;
                case CalibrationGuiState.Ended:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        CalibrationFinished = true;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        
        public void HandleConcentrationCalculated(CalibrationData totalCells, CalibrationData originalConcentration, CalibrationData adjustedConcentration)
        {
            ResetGraphList();

            var graphViewList = _concentrationService.GetCalibrationData(
                totalCells, originalConcentration, adjustedConcentration);

            DispatcherHelper.ApplicationExecute(() =>
            {
                GraphViewList = graphViewList.ToObservableCollection();
                SelectedGraphIndex = 0;
                GraphExpandCommand.RaiseCanExecuteChanged();
            });
        }

        #endregion

        #region Commands

        #region OnLoaded

        private RelayCommand _onLoaded;
        public RelayCommand OnLoaded => _onLoaded ?? (_onLoaded = new RelayCommand(PerformOnLoaded));

        private void PerformOnLoaded()
        {
            // Attention, developers:
            // You can use this method to put the graphs in a state that makes it easy to test. 
            // Need to check the graphs, but don't want to run a full calibration? Just use the code below
            // and the graphs will magically appear!

            //var graphViewList = GetLineGraphList();
            //DispatcherHelper.ApplicationExecute(() =>
            //{
            //    GraphViewList = graphViewList.ToObservableCollection();
            //    SelectedGraphIndex = 0;
            //    GraphExpandCommand.RaiseCanExecuteChanged();
            //});

            // But, wait! There's more!
            // If you want some fake calibration calculation data, you can use this:

            //GetFakeData(out var totalCells, out var originalConcentration, out var adjustedConcentration);
            //HandleConcentrationCalculated(totalCells, originalConcentration, adjustedConcentration);
            //CalibrationFinished = true;
        }

        #endregion

        #region Graph Expand Command

        private RelayCommand _graphExpandCommand;
        public RelayCommand GraphExpandCommand => _graphExpandCommand ?? (_graphExpandCommand = new RelayCommand(PerformExpandGraph, CanPerformExpandGraph));

        private bool CanPerformExpandGraph()
        {
            return CalibrationFinished;
        }

        private void PerformExpandGraph()
        {
            var graphList = GraphViewList?.Cast<object>()?.ToList();
            var index = SelectedGraphIndex;
            var graph = _selectedGraph;
            var args = new ExpandedImageGraphEventArgs(ExpandedContentType.ScatterChart, ApplicationConstants.ImageViewRightClickMenuImageFitSize, graphList, graph, index);
            DialogEventBus.ExpandedImageGraph(this, args);
        }

        #endregion

        #endregion

        #region Private Helper Methods

        private void ResetGraphList()
        {
            GraphViewList = new ObservableCollection<LineGraphDomain>
            {
                new LineGraphDomain{GraphName = LanguageResourceHelper.Get("LID_GraphLabel_Totalcells")}, 
                new LineGraphDomain{GraphName = LanguageResourceHelper.Get("LID_GraphLabel_OriginalConcentration")}, 
                new LineGraphDomain{GraphName = LanguageResourceHelper.Get("LID_Graph_AdjustConc")}, 
                new LineGraphDomain{GraphName = LanguageResourceHelper.Get("LID_Graph_Original")}
            };

            SelectedGraphIndex = -1; // this makes the graphs Visibility set to Collapsed
        }
        
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

        private IList<LineGraphDomain> GetLineGraphList()
        {
            var cells = JsonConvert.DeserializeObject<LineGraphDomain>(@"{'ExpandCommand':null,'AcceptanceLowerLimit':0.0,'AcceptanceUpperLimit':0.0,'IsExpandableView':false,'SecondaryTrendLegendName':null,'PrimaryTrendLegendName':null,'PrimaryTrendLabel':'y = 9272.8105x\nR² = -0.570','SecondaryTrendLabel':null,'GraphName':'Cell count','LegendTitle':null,'XAxisName':'Assay value','YAxisName':'Cell count','IsMultiAxisEnable':false,'PrimaryLegendName':null,'SecondaryLegendName':null,'PrimaryTrendPoints':[{'Key':0.0,'Value':0.0},{'Key':0.0,'Value':0.0}],'SecondaryTrendPoints':null,'GraphDetailList':[{'Key':2.0,'Value':575.0},{'Key':2.0,'Value':575.0},{'Key':2.0,'Value':576.0},{'Key':2.0,'Value':574.0},{'Key':2.0,'Value':575.0},{'Key':2.0,'Value':575.0},{'Key':2.0,'Value':574.0},{'Key':2.0,'Value':576.0},{'Key':2.0,'Value':575.0},{'Key':2.0,'Value':575.0},{'Key':4.0,'Value':576.0},{'Key':4.0,'Value':574.0},{'Key':4.0,'Value':575.0},{'Key':4.0,'Value':575.0},{'Key':4.0,'Value':574.0},{'Key':10.0,'Value':576.0},{'Key':10.0,'Value':575.0},{'Key':10.0,'Value':575.0}],'MultiGraphDetailList':[]}");
            cells.PrimaryTrendPoints = null;
            cells.SecondaryTrendPoints = null;
            cells.MultiGraphDetailList = null;

            var og = JsonConvert.DeserializeObject<LineGraphDomain>(@"{'ExpandCommand':null,'AcceptanceLowerLimit':0.0,'AcceptanceUpperLimit':0.0,'IsExpandableView':false,'SecondaryTrendLegendName':null,'PrimaryTrendLegendName':null,'PrimaryTrendLabel':'y = 25.1979x\nR² = -0.570','SecondaryTrendLabel':null,'GraphName':'Original concentration','LegendTitle':null,'XAxisName':'Assay value','YAxisName':'Total (x10^6) cells/mL','IsMultiAxisEnable':false,'PrimaryLegendName':null,'SecondaryLegendName':null,'PrimaryTrendPoints':[{'Key':0.0,'Value':0.0},{'Key':0.0,'Value':0.0}],'SecondaryTrendPoints':null,'GraphDetailList':[{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':10.0,'Value':0.21},{'Key':10.0,'Value':0.21},{'Key':10.0,'Value':0.21}],'MultiGraphDetailList':[]}");
            og.PrimaryTrendPoints = null;
            og.SecondaryTrendPoints = null;
            og.MultiGraphDetailList = null;

            var adj = JsonConvert.DeserializeObject<LineGraphDomain>(@"{'ExpandCommand':null,'AcceptanceLowerLimit':0.0,'AcceptanceUpperLimit':0.0,'IsExpandableView':false,'SecondaryTrendLegendName':null,'PrimaryTrendLegendName':null,'PrimaryTrendLabel':'y = 1x\nR² = -0.570','SecondaryTrendLabel':null,'GraphName':'Adjusted concentration','LegendTitle':null,'XAxisName':'Assay value','YAxisName':'Total (x10^6) cells/mL','IsMultiAxisEnable':false,'PrimaryLegendName':null,'SecondaryLegendName':null,'PrimaryTrendPoints':[{'Key':0.0,'Value':0.0},{'Key':0.0,'Value':0.0}],'SecondaryTrendPoints':null,'GraphDetailList':[{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.34},{'Key':2.0,'Value':5.32},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.32},{'Key':2.0,'Value':5.34},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':4.0,'Value':5.34},{'Key':4.0,'Value':5.32},{'Key':4.0,'Value':5.33},{'Key':4.0,'Value':5.33},{'Key':4.0,'Value':5.32},{'Key':10.0,'Value':5.34},{'Key':10.0,'Value':5.33},{'Key':10.0,'Value':5.33}],'MultiGraphDetailList':[]}");
            adj.PrimaryTrendPoints = null;
            adj.SecondaryTrendPoints = null;
            adj.MultiGraphDetailList = null;

            var ogAdj = JsonConvert.DeserializeObject<LineGraphDomain>(@"{'ExpandCommand':null,'AcceptanceLowerLimit':0.0,'AcceptanceUpperLimit':0.0,'IsExpandableView':false,'SecondaryTrendLegendName':'Adjusted concentration','PrimaryTrendLegendName':'Original concentration','PrimaryTrendLabel':'y = 1x\nR² = -0.5697','SecondaryTrendLabel':null,'GraphName':'Original and adjusted','LegendTitle':null,'XAxisName':'Assay value','YAxisName':'Total (x10^6) cells/mL','IsMultiAxisEnable':true,'PrimaryLegendName':'Original Conc','SecondaryLegendName':'Adjusted concentration','PrimaryTrendPoints':[{'Key':0.0,'Value':0.0},{'Key':0.0,'Value':0.0}],'SecondaryTrendPoints':[{'Key':0.0,'Value':0.0},{'Key':0.0,'Value':0.0}],'GraphDetailList':[{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':2.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':4.0,'Value':0.21},{'Key':10.0,'Value':0.21},{'Key':10.0,'Value':0.21},{'Key':10.0,'Value':0.21}],'MultiGraphDetailList':[{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.34},{'Key':2.0,'Value':5.32},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.32},{'Key':2.0,'Value':5.34},{'Key':2.0,'Value':5.33},{'Key':2.0,'Value':5.33},{'Key':4.0,'Value':5.34},{'Key':4.0,'Value':5.32},{'Key':4.0,'Value':5.33},{'Key':4.0,'Value':5.33},{'Key':4.0,'Value':5.32},{'Key':10.0,'Value':5.34},{'Key':10.0,'Value':5.33},{'Key':10.0,'Value':5.33}]}");
            ogAdj.PrimaryTrendPoints = null;
            ogAdj.SecondaryTrendPoints = null;
            ogAdj.SecondaryTrendPoints = null;

            return new List<LineGraphDomain> { cells, og, adj, ogAdj };
        }

        #endregion
    }
}