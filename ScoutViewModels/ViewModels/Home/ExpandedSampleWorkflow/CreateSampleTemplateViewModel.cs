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

        public override bool IsFastModeEnabled => base.IsFastModeEnabled && _createSampleSetDialogViewModel.SelectedPlateType != SubstrateType.AutomationCup;

        public override object Clone()
        {
            var newObject = _viewModelFactory.CreateCreateSampleTemplateViewModel();
            CopyValues(newObject);
            return newObject;
        }

    }
}