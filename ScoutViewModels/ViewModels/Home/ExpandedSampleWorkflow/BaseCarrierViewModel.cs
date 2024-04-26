using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Interfaces;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class BaseCarrierViewModel : BaseViewModel
    {
        protected BaseCarrierViewModel(SampleTemplateViewModel sampleTemplate, SampleSetViewModel sampleSet,
            List<CellTypeQualityControlGroupDomain> cellTypeQualityControlGroup, ISolidColorBrushService colorBrushService,
            RunOptionSettingsModel runOptionSettings)
        {
            SampleTemplate = sampleTemplate;
            SampleSet = sampleSet;
            _cachedCellTypeQualityControlList = cellTypeQualityControlGroup;
            ColorBrushService = colorBrushService;
            RunOptionSettings = runOptionSettings;
        }

        #region Properties & Fields

        private List<CellTypeQualityControlGroupDomain> _cachedCellTypeQualityControlList;

        protected ISolidColorBrushService ColorBrushService;
        public RunOptionSettingsModel RunOptionSettings;

        public SampleWellViewModel LastSampleWellAdded;

        public SampleTemplateViewModel SampleTemplate { get; set; }
        public SampleSetViewModel SampleSet { get; set; }
        public SubstrateType CarrierType { get; protected set; }

        public event EventHandler WellWasClicked;

        public ObservableCollection<SampleWellViewModel> SampleWellButtons
        {
            get { return GetProperty<ObservableCollection<SampleWellViewModel>>(); }
            set
            {
                SetProperty(value);
                OnSampleWellButtonsUpdated();
            }
        }

        #endregion

        #region Commands

        #region Sample Well Clicked

        private RelayCommand<SampleWellViewModel> _onClicked;
        public RelayCommand<SampleWellViewModel> OnClicked => _onClicked ?? (_onClicked = new RelayCommand<SampleWellViewModel>(PerformOnClicked, CanPerformOnClicked));

        protected virtual bool CanPerformOnClicked(SampleWellViewModel sampleWellVm)
        {
            return true;
        }

        protected virtual void PerformOnClicked(SampleWellViewModel sampleWellVm)
        {
            if (sampleWellVm == null ||
                sampleWellVm.WellState == SampleWellState.UsedInAnotherSet ||
                sampleWellVm.WellState == SampleWellState.Processing ||
                sampleWellVm.WellState == SampleWellState.Blocked)
            {
                return;
            }

            if (sampleWellVm.WellState == SampleWellState.Available)
            {
                CreateSampleVmForWell(sampleWellVm);
            }
            else if (sampleWellVm.WellState == SampleWellState.UsedInCurrentSet)
            {
                DeselectSampleWell(sampleWellVm);
            }

            InvokeWellWasClicked();
            ClearAllSampleWells.RaiseCanExecuteChanged();
        }

        #endregion

        #region Clear All Samples Command

        private RelayCommand _clearAllSampleWells;
        public RelayCommand ClearAllSampleWells => _clearAllSampleWells ?? (_clearAllSampleWells = new RelayCommand(PerformClearAllSamples, CanPerformClearAllSamples));

        protected virtual bool CanPerformClearAllSamples()
        {
            return SampleWellButtons.Any(s => s.WellState == SampleWellState.UsedInCurrentSet);
        }

        protected virtual void PerformClearAllSamples()
        {
            if (DialogEventBus.DialogBoxYesNo(this, LanguageResourceHelper.Get("LID_MSG_ClearDefinedCell")) == true)
            {
                ClearSampleSet();
            }
        }

        #endregion

        #endregion

        #region Virtual Methods

        protected virtual void OnSampleWellButtonsUpdated() { }

        protected virtual void SetWellPositions() { }

        public virtual IList<SampleWellViewModel> GetSampleWellButtons()
        {
            return SampleWellButtons;
        }

        #endregion

        #region Public Methods

        public void ClearSampleSet()
        {
            foreach (var w in SampleWellButtons)
            {
                if (w.WellState == SampleWellState.UsedInCurrentSet)
                {
                    w.Sample = null;
                    w.WellState = SampleWellState.Available;
                }
            }

            SampleTemplate.SequentialNamingItems.ResetSequenceNumbering();

            WellWasClicked?.Invoke(this, EventArgs.Empty);
            ClearAllSampleWells.RaiseCanExecuteChanged();
        }

        public bool CarouselHasNewItems()
        {
            return SampleWellButtons.Any(w => w.WellState == SampleWellState.UsedInCurrentSet);
        }

        public bool CarouselHasAnyItems()
        {
            return SampleWellButtons.Any(w => w.WellState == SampleWellState.UsedInCurrentSet ||
                                              w.WellState == SampleWellState.UsedInAnotherSet ||
                                              w.WellState == SampleWellState.Processing);
        }

        public List<SamplePosition> OccupiedCarouselPositions()
        {
            return SampleWellButtons.Where(w => w.WellState == SampleWellState.UsedInCurrentSet ||
                                                w.WellState == SampleWellState.UsedInAnotherSet ||
                                                w.WellState == SampleWellState.Processing)
                .Select(w => w.SamplePosition).ToList();
        }

        public List<SamplePosition> AvailableCarouselPositions()
        {
            return SampleWellButtons.Where(w => w.WellState == SampleWellState.Available)
                .Select(w => w.SamplePosition).ToList();
        }

        #endregion

        #region Protected Methods

        protected void InvokeWellWasClicked()
        {
            WellWasClicked?.Invoke(this, EventArgs.Empty);
        }

        public void CreateSampleVmForWell(SampleWellViewModel sampleWellVm)
        {
            var sample = new SampleViewModel(RunOptionSettings, qcAndCellTypes: _cachedCellTypeQualityControlList);

            if (SampleTemplate.UseSequencing)
            {
                if (LastSampleWellAdded?.Sample?.SampleName != null &&
                    !LastSampleWellAdded.Sample.SampleName.Equals(SampleTemplate.SequentialNamingItems.Previous()))
                {
                    // the last sample added has had its name changed -- decrement the sequence number
                    SampleTemplate.SequentialNamingItems.DecrementSequenceNumber();
                }

                sample.SequentialSampleNumberPart = SampleTemplate.SequentialNamingItems.GetIntegerItem().CurrentSeqNumber;
                sample.SampleName = SampleTemplate.SequentialNamingItems.Next();
                sample.UsingSequentialSampleName = true;
            }
            else
            {
                sample.SampleName = SampleTemplate.SequentialNamingItems.GetBaseString();
                sample.UsingSequentialSampleName = false;
            }

            sample.SamplePosition = sampleWellVm.SamplePosition;
            sample.SubstrateType = sampleWellVm.SamplePosition.GetSubstrateType();
            sample.QcOrCellType = SampleTemplate.QcCellType;
            sample.Dilution = SampleTemplate.Dilution;
            sample.SampleTag = SampleTemplate.SampleTag;
            sample.WashType = SampleTemplate.WashType;
            sample.SampleSetName = SampleSet.SampleSetName;
            sample.SampleSetUid = SampleSet.Uuid;
            sample.Username = SampleSet.CreatedByUser;
            sample.SampleStatus = SampleStatus.NotProcessed;
            sample.AdvancedSampleSettings = (AdvancedSampleSettingsViewModel) SampleTemplate.AdvancedSampleSettings.Clone();
            
            sampleWellVm.Sample = sample;
            sampleWellVm.WellState = SampleWellState.UsedInCurrentSet;
            LastSampleWellAdded = sampleWellVm;
        }

        protected void DeselectSampleWell(SampleWellViewModel sampleWellVm)
        {
            if (SampleTemplate.UseSequencing &&
                (sampleWellVm.Sample.UsingSequentialSampleName || LastSampleWellAdded == sampleWellVm))
            {
                // only decrement if the currently de-selected sample's number is is the highest one in the sequence
                var prevNum = SampleTemplate.SequentialNamingItems.GetCurrentSeqNumber() - 1;
                if (sampleWellVm.Sample.SequentialSampleNumberPart == prevNum)
                {
                    SampleTemplate.SequentialNamingItems.DecrementSequenceNumber();
                }
            }

            sampleWellVm.Sample.Dispose();
            sampleWellVm.Sample = null;
            sampleWellVm.WellState = SampleWellState.Available;
        }

        #endregion
    }
}