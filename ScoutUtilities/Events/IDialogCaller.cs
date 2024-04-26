using ScoutUtilities.CustomEventArgs;

namespace ScoutUtilities.Events
{
    /// <summary>
    /// This interface/class exists so that we can mock the dialogs that can
    /// stop unit tests from working correctly.
    /// 
    /// If you write a unit test that calls a function which makes a dialog,
    /// you can add that particular dialog to this interface, then use Ninject in
    /// the dialog caller class (instead of directly calling DialogEventBus).
    /// Now you can mock this interface in your unit test to have it return what you need.
    ///
    /// For an example, check out AcupConcentrationSlopeViewModel.PerformStartACupConcSample()
    /// and its unit test TestDoesShowACupStopButtonIfRunning().
    /// </summary>
    public interface IDialogCaller
    {
        bool? DialogBoxOkCancel(object caller, string message, string title = null);
        bool? DialogBoxOk(object caller, string message, string title = null);
        bool? OpenFolderSelectDialog(object caller, FolderSelectDialogEventArgs args);
    }
}