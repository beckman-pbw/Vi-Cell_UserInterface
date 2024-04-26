using ScoutDomains;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public sealed class CarouselViewModel : BaseCarrierViewModel
    {
        public CarouselViewModel(SampleTemplateViewModel sampleTemplate, SampleSetViewModel sampleSet,
            List<CellTypeQualityControlGroupDomain> cellTypeQualityControlGroup, bool hideWells,
            ISolidColorBrushService colorBrushService, RunOptionSettingsModel runOptionSettings)
        : base(sampleTemplate, sampleSet, cellTypeQualityControlGroup, colorBrushService, runOptionSettings)
        {
            _wrapAroundIndexer = new WrapAroundIndexer(CarouselModel.Instance.SampleWells.Count, false);
            _hideWells = hideWells;
            CarrierType = SubstrateType.Carousel;

            SetWellPositions();
        }

        #region Properties & Fields

        private readonly bool _hideWells;
        private readonly WrapAroundIndexer _wrapAroundIndexer;
        
        #endregion

        #region Commands

        #region Rotate Carousel Command

        private RelayCommand _rotateCarousel;
        public RelayCommand RotateCarousel => _rotateCarousel ?? (_rotateCarousel = new RelayCommand(PerformRotateCarousel));

        private void PerformRotateCarousel()
        {
            CarouselModel.Instance.IncrementTopCarouselPosition();

            SetWellPositions(SampleWellButtons);

            var newTopPosition = SampleWellButtons.First(w => w.SamplePosition.Column == CarouselModel.Instance.TopCarouselPosition);
            var oldTopPosition = SampleWellButtons.First(w => w.SamplePosition.Column == _wrapAroundIndexer.GetPreviousIndex(CarouselModel.Instance.TopCarouselPosition));

            if (newTopPosition?.WellState == SampleWellState.UsedInCurrentSet || newTopPosition?.WellState == SampleWellState.UsedInAnotherSet)
            {
                newTopPosition.WellState = SampleWellState.Processing;
            }

            if (oldTopPosition?.WellState == SampleWellState.Processing)
            {
                oldTopPosition.WellState = SampleWellState.Available;
                CarouselModel.Instance.SampleWells[oldTopPosition.SamplePosition.Column].Sample = null;
            }

            ClearAllSampleWells.RaiseCanExecuteChanged();
        }

        #endregion

        #endregion

        #region Override Methods

        protected override void OnSampleWellButtonsUpdated()
        {
            // nothing extra to do for Carousel
        }

        protected override void SetWellPositions()
        {
            var list = new List<SampleWellViewModel>();
            for (var i = ApplicationConstants.StartingIndexOfCarousel; i <= CarouselModel.Instance.SampleWells.Count; i++)
            {
                var wellModel = CarouselModel.Instance.SampleWells[i];
                var wellViewModel = SampleWellButtons?.FirstOrDefault(s => s.SamplePosition.Column == i)
                                    ?? new SampleWellViewModel(RowPosition.Z, (byte) i, ColorBrushService);
                if (wellModel.IsEmpty)
                {
                    wellViewModel.WellState = SampleWellState.Available;
                }
                else
                {
                    if (wellViewModel.WellState != SampleWellState.Processing)
                        wellViewModel.WellState = SampleWellState.UsedInAnotherSet;
                }

                list.Add(wellViewModel);
            }

            SetWellPositions(list);
        }

        #endregion

        #region Private Methods

        private void SetWellPositions(IList<SampleWellViewModel> initialList)
        {
            // add the items in the correct order based on what position is at the TOP of the carousel
            var newCarouselList = new List<SampleWellViewModel>();
            for (var i = CarouselModel.Instance.TopCarouselPosition; i <= CarouselModel.Instance.SampleWells.Count; i++)
            {
                newCarouselList.Add(initialList.FirstOrDefault(b => b.SamplePosition.Column == i));
            }

            for (var j = ApplicationConstants.StartingIndexOfCarousel; j < CarouselModel.Instance.TopCarouselPosition; j++)
            {
                newCarouselList.Add(initialList.FirstOrDefault(b => b.SamplePosition.Column == j));
            }

            //
            // If a work list is running (paused), 
            // Hide the current well and some wells to the left and right.
            // This discourages the use of wells that are physically 
            // difficult or impossible to get to. 
            //
            if (_hideWells)
            {
                int currIdx = CarouselModel.Instance.TopCarouselPosition;
                List<int> blockIndexes = new List<int>();
                blockIndexes.Add(currIdx);
                blockIndexes.AddRange(_wrapAroundIndexer.GetNextIndexes(currIdx, ApplicationConstants.CarouselBlockWellsLeft));
                blockIndexes.AddRange(_wrapAroundIndexer.GetPreviousIndexes(currIdx, ApplicationConstants.CarouselBlockWellsRight));

                foreach (var wellVm in newCarouselList.Where(w => blockIndexes.Contains(w.SamplePosition.Column)))
                {
                    wellVm.WellState = SampleWellState.Blocked;
                }
            }

            SampleWellButtons = new ObservableCollection<SampleWellViewModel>(newCarouselList);
        }

        #endregion
    }
}