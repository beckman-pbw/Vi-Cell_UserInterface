using ScoutDomains;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class AutomationCupViewModel : BaseCarrierViewModel
    {
        public AutomationCupViewModel(SampleTemplateViewModel sampleTemplate, SampleSetViewModel sampleSet,
            List<CellTypeQualityControlGroupDomain> cellTypeQualityControlGroup, ISolidColorBrushService colorBrushService,
            RunOptionSettingsModel runOptionSettings)
            : base(sampleTemplate, sampleSet, cellTypeQualityControlGroup, colorBrushService, runOptionSettings)
        {
            CarrierType = SubstrateType.AutomationCup;
            SetWellPositions();
        }

        #region Override Methods

        protected override void OnSampleWellButtonsUpdated()
        {
            // nothing extra to do for Automation Cup
        }

        protected sealed override void SetWellPositions()
        {
            // there is only 1 well for Automation Cup
            var list = new List<SampleWellViewModel>();
            
            var well = new SampleWellViewModel(RowPosition.Y, 1, ColorBrushService);
            well.WellState = SampleWellState.Available;
            list.Add(well);
            
            SampleWellButtons = new ObservableCollection<SampleWellViewModel>(list);
        }

        #endregion
    }
}