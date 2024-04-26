using System.Windows;
using System.Windows.Media;
using ScoutLanguageResources;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class InfoDialogViewModel : BaseDialogViewModel
    {
        public InfoDialogViewModel(InfoDialogEventArgs args, Window parentWindow,IAutomationSettingsService automationSettingsService = null) : base(args, parentWindow)
        {
            SizeToContent = true;
            DialogTitle = LanguageResourceHelper.Get("LID_Label_EnhancedSampleWorkflow");
            DialogIconDrawBrush = (DrawingBrush) Application.Current.Resources["QuestionTitleBarIcon"];
            
            var automationSettingsModel = automationSettingsService ?? new AutomationSettingsService();
            var autoConfig = automationSettingsModel.GetAutomationConfig();
            IsACupEnabled = Misc.ByteToBool(autoConfig.ACupIsEnabled);
        }

        public bool IsACupEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value);}
        }

    }
}