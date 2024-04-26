using ScoutDomains;
using ScoutModels.Home.ExpandedSampleWorkflow;
using ScoutUtilities.Enums;
using System.Collections.ObjectModel;
using HawkeyeCoreAPI.Facade;
using ScoutDomains.Analysis;
using ScoutModels.Settings;
using ScoutServices;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;

namespace ScoutViewModels.ViewModels.Home.ExpandedSampleWorkflow
{
    /// <summary>
    /// The OrphanSampleTemplateViewModel is used to for orphan samples added to the carousel after a run has started. The
    /// SampleTemplateUserControl is populated by this view model, but all the controls are disabled.
    /// When samples are being run, this control is shown, and the same control, but populated
    /// by the UserSampleTemplateViewModel is hidden. When the system is in an Idle state,
    /// this control is hidden and the user control is visible.
    /// </summary>
    public class OrphanSampleTemplateViewModel : SampleTemplateViewModel
    {
        public OrphanSampleTemplateViewModel(CellTypeFacade cellTypeManager, RunOptionSettingsModel runOptions, ISampleProcessingService sampleProcessingService, IScoutViewModelFactory viewModelFactory) :
            base(runOptions, sampleProcessingService, viewModelFactory, cellTypeManager)
        {
            IsEnabled = false;
        }

        public override object Clone()
        {
            var newObject = _viewModelFactory.CreateOrphanSampleTemplateViewModel();
            CopyValues(newObject);
            return newObject;
        }


        /// <summary>
        /// Called when the Run Samples button is pressed. The values of the UserSampleTemplateViewModel are
        /// copied into this view model. These are the values that will be used for all orphan samples.
        /// </summary>
        /// <param name="copy"></param>
        public void CopyFrom(SampleTemplateViewModel copy)
        {
            QcCellTypes = new ObservableCollection<CellTypeQualityControlGroupDomain>(copy.QcCellTypes);
            QcCellType = copy.QcCellType;
            Dilution = copy.Dilution;
            WashTypes = new ObservableCollection<SamplePostWash>(copy.WashTypes);
            WashType = copy.WashType;
            SampleTag = copy.SampleTag;;
            UseSequencing = copy.UseSequencing;
            SequentialNamingItems = (SequentialNamingSetViewModel) copy.SequentialNamingItems.Clone();
            AdvancedSampleSettings = (AdvancedSampleSettingsViewModel) copy.AdvancedSampleSettings.Clone();
            User = copy.User;

            var baseTextString = copy.SequentialNamingItems.GetTextItem()?.BaseTextString;
            DisplayedSampleName = SampleSetModel.CleanSequentialNamingDisplayName(baseTextString);
        }
    }
}