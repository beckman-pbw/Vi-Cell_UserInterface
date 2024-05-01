using System.Collections.ObjectModel;
using System.Web.Configuration;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.Dialogs;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;

namespace ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow
{
    public class CreateSampleTemplateViewModel : SampleTemplateViewModel
    {
        private readonly CreateSampleSetDialogViewModel _createSampleSetDialogViewModel;

        public CreateSampleTemplateViewModel(SampleTemplateViewModel sampleTemplateViewModel, CreateSampleSetDialogViewModel createSampleSetDialogViewModel) : base(sampleTemplateViewModel)
        {
            _createSampleSetDialogViewModel = createSampleSetDialogViewModel;
        }

//TODO: LH6531-6757 - CHM / Automation cup now use the wash type to toggle an additional sampling script type (no internal dilution)
        public override bool IsFastModeEnabled => base.IsFastModeEnabled && _createSampleSetDialogViewModel.SelectedPlateType != SubstrateType.AutomationCup;

        public override object Clone()
        {
            var newObject = _viewModelFactory.CreateCreateSampleTemplateViewModel();
            CopyValues(newObject);
            return newObject;
        }

    }
}