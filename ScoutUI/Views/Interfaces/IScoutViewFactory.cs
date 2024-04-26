using ScoutUI.Views.Dialogs;
using System.Windows;

namespace ScoutUI.Views
{
    public interface IScoutViewFactory
    {
        DialogEventManager CreateDialogEventManager(Window parent);
    }
}
