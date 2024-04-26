using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System;
using ScoutModels.Interfaces;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class EmptySampleTubesDialogViewModel : BaseDialogViewModel
    {
        private readonly IInstrumentStatusService _instrumentStatusService;
        public EmptySampleTubesDialogViewModel(IInstrumentStatusService instrumentStatusService,  EmptySampleTubesEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_HELP_EmptyWasteTube");
            _instrumentStatusService = instrumentStatusService;
            if (args.TubeCapacity != null)
            {
                ExtraTitleBarText = $"{LanguageResourceHelper.Get("LID_Label_Remainingwastetubebincapacity")} = {args.TubeCapacity}";
            }
        }

        protected override void OnAccept()
        {
            try
            {
                var hawkeyeError = _instrumentStatusService.SampleTubeDiscardTrayEmptied();

                if (hawkeyeError.Equals(HawkeyeError.eSuccess))
                {
                    PostToMessageHub(LanguageResourceHelper.Get("LID_StatusBar_RemainingWasteTubeBinCapacit"));
                    Close(true);
                }
                else
                {
                    ApiHawkeyeMsgHelper.ErrorCommon(hawkeyeError);
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERROR_ON_CLEAN_BIN"));
            }
        }
    }
}