using ScoutDomains;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Interfaces;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Navigation;
using HawkeyeCoreAPI;


namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public sealed class PlateViewModel : BaseCarrierViewModel
    {
        public PlateViewModel(SampleTemplateViewModel sampleTemplate, SampleSetViewModel sampleSet,
            List<CellTypeQualityControlGroupDomain> cellTypeQualityControlGroup, bool useRowSort,
            ISolidColorBrushService colorBrushService, RunOptionSettingsModel runOptionSettings)
        : base(sampleTemplate, sampleSet, cellTypeQualityControlGroup, colorBrushService, runOptionSettings)
        {
            UsingRowWiseSort = useRowSort;
            AllWellsChecked = false;
            AllWellsButtonEnabled = true;
            SelectAllHeaderButton = new SampleGridHeaderViewModel(string.Empty, colorBrushService);
            CarrierType = SubstrateType.Plate96;

            SetWellPositions();
        }

        #region Properties & Fields

        public ObservableCollection<SampleWellViewModel> SortedSampleWellButtons
        {
            get { return GetProperty<ObservableCollection<SampleWellViewModel>>(); }
            set { SetProperty(value); }
        }

        public SampleGridHeaderViewModel SelectAllHeaderButton
        {
            get { return GetProperty<SampleGridHeaderViewModel>(); }
            set { SetProperty(value); }
        }

        public bool AllWellsChecked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool AllWellsButtonEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool UsingRowWiseSort
        {
            get { return GetProperty<bool>(); }
            set
            {
                var changed = value != GetProperty<bool>();
                if (changed)
                {
                    SetProperty(value); // set before the call to SortSampleSampleWellButtons()
                    SortSampleSampleWellButtons();
                }
            }
        }

        #endregion

        #region Commands

        #region Plate Row/Col Header Clicked

        private RelayCommand<SampleGridHeaderViewModel> _onHeaderClicked;
        public RelayCommand<SampleGridHeaderViewModel> OnHeaderClicked => _onHeaderClicked ?? (_onHeaderClicked = new RelayCommand<SampleGridHeaderViewModel>(PerformOnHeaderClicked));

        private void PerformOnHeaderClicked(SampleGridHeaderViewModel headerButton)
        {
            if (headerButton == null || !headerButton.IsEnabled) return;

            if (int.TryParse(headerButton.Label, out var i))
            {
                var samples = SampleWellButtons.Where(w => w.SamplePosition.Column.Equals((byte) i)).ToList();
                UpdatePlateSampleWellsSelections(samples);
            }
            else if (char.TryParse(headerButton.Label, out var c))
            {
                var samples = SampleWellButtons.Where(w => w.SamplePosition.Row.Equals(c)).ToList();
                UpdatePlateSampleWellsSelections(samples);
            }

            InvokeWellWasClicked();
            ClearAllSampleWells.RaiseCanExecuteChanged();
        }

        #endregion

        #region All Well Button Click

        private RelayCommand _onAllButtonClicked;
        public RelayCommand OnAllButtonClicked => _onAllButtonClicked ?? (_onAllButtonClicked = new RelayCommand(PerformOnAllButtonClicked));

        private void PerformOnAllButtonClicked()
        {
            var samples = new List<SampleWellViewModel>(SampleWellButtons.ToList());
            if (SampleTemplate.UseSequencing)
            {
                SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();
                if (UsingRowWiseSort)
                {
                    samples = samples
                        .OrderBy(w => (uint)RowPositionHelper.GetRowPosition(w.SamplePosition.Row))
                        .ThenBy(w => w.SamplePosition.Column)
                        .ToList();
                }
                else
                {
                    samples = samples
                        .OrderBy(w => w.SamplePosition.Column)
                        .ThenBy(w => (uint)RowPositionHelper.GetRowPosition(w.SamplePosition.Row))
                        .ToList();
                }
            }
            UpdatePlateSampleWellsSelections(samples);
            
            InvokeWellWasClicked();
            ClearAllSampleWells.RaiseCanExecuteChanged();
        }

        #endregion

        #endregion

        #region Override Methods

        public override IList<SampleWellViewModel> GetSampleWellButtons()
        {
            return SortedSampleWellButtons;
        }

        protected override void OnSampleWellButtonsUpdated()
        {
            SortSampleSampleWellButtons();
        }

        protected override void SetWellPositions()
        {
            var list = new List<SampleWellViewModel>();
            for (var i = 0; i < ApplicationConstants.PlateNumRowsCount; i++)
            {
                for (var j = 0; j < ApplicationConstants.PlateNumColumnsCount; j++)
                {
                    var well = new SampleWellViewModel((RowPosition) i, (byte) (j + 1), ColorBrushService);
                    well.WellState = SampleWellState.Available;
                    list.Add(well);
                }
            }

            SampleWellButtons = new ObservableCollection<SampleWellViewModel>(list);
        }

        #endregion

        #region Private Methods

        private void UpdatePlateSampleWellsSelections(IList<SampleWellViewModel> samples)
        {
            if (samples == null || !samples.Any()) return;

            var selectAll = samples.Any(s => !s.IsChecked);

            foreach (var well in samples)
            {
                if (selectAll)
                {
                    CreateSampleVmForWell(well);
                }
                else if (well.WellState == SampleWellState.UsedInCurrentSet)
                {
                    DeselectSampleWell(well);
                }
            }
        }

        private void SortSampleSampleWellButtons()
        {
            if (SampleWellButtons == null || !SampleWellButtons.Any()) return;

            if (UsingRowWiseSort)
            {
                SortedSampleWellButtons = SampleWellButtons
                    .OrderBy(w => (uint)RowPositionHelper.GetRowPosition(w.SamplePosition.Row))
                    .ThenBy(w => w.SamplePosition.Column)
                    .ToObservableCollection();
            }
            else
            {
                SortedSampleWellButtons = SampleWellButtons
                    .OrderBy(w => w.SamplePosition.Column)
                    .ThenBy(w => (uint)RowPositionHelper.GetRowPosition(w.SamplePosition.Row))
                    .ToObservableCollection();
            }
        }

        #endregion
    }
}