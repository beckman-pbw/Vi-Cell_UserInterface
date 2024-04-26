using log4net;
using ScoutUI.Views.Dialogs;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScoutUI
{
    public class BaseEventManager : Disposable
    {
        public BaseEventManager(Window parent)
        {
            ParentWindow = parent;
            ActiveWindows = new List<Window>();
        }

        #region Properties & Fields

        protected Window ParentWindow;
        protected List<Window> ActiveWindows;

        #endregion

        protected void ShowDialog(Func<Dialog> createDialog, BaseDialogViewModel vm, BaseDialogEventArgs args)
        {
            if (args?.IsModal == true)
            {
                ShowModalDialog(createDialog, vm, args);
            }
            else
            {
                ShowUnattachedDialog(createDialog, vm, args);
            }
        }

        protected bool? ShowModalDialog(Func<Dialog> createDialog, BaseDialogViewModel vm, BaseDialogEventArgs args)
        {
            if (ParentWindow.Dispatcher.CheckAccess())
            {
                Dialog dialog = null;
                try
                {
                    dialog = createDialog();
                    lock (ActiveWindows)
                    {
                        ActiveWindows.Add(dialog);
                    }

                    if (ParentWindow.IsVisible) dialog.Owner = ParentWindow;

                    using (args?.CancellationToken?.Register(() => vm.Close(null)))
                    {
                        dialog.ShowDialog();
                    }

                    lock (ActiveWindows)
                    {
                        ActiveWindows.Remove(dialog);
                    }

                    if (args != null) args.DialogResult = vm.DialogResult;
                }
                catch (Exception ex)
                {
                    Log.Error($"{dialog?.GetType().Name} had an error (VM: {vm.GetType().Name})", ex);
                }
                return vm.DialogResult;
            }
            else
            {
                ParentWindow.Dispatcher.Invoke(() =>
                {
                    ShowModalDialog(createDialog, vm, args);
                });
                return vm.DialogResult;
            }
        }

        protected void ShowUnattachedDialog(Func<Dialog> createDialog, BaseDialogViewModel vm, BaseDialogEventArgs args)
        {
            ParentWindow.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, (Action) (() =>
            {
                var dialog = createDialog();
                var cancelRegistration = args?.CancellationToken?.Register(() => vm.Close(null));

                if (ParentWindow.IsVisible)
                    dialog.Owner = ParentWindow;

                dialog.Closed += (s, e) =>
                {
                    Task.Run(() => cancelRegistration?.Dispose());
                    args?.OnCloseCallbackAction?.Invoke(vm.DialogResult);
                };

                try
                {
                    dialog.Show();
                    dialog.Activate();
                }
                catch (Exception ex)
                {
                    Log.Error("Error with Show()/Activate() new dialog window", ex);
                    try
                    {
                        dialog?.Close();
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error with Close() new dialog window inside previous exception", e);
                    }
                }
            }));
        }
    }
}