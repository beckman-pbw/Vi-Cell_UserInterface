using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Shapes;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.Service;

namespace ScoutUI.Views.Service
{
    /// <summary>
    /// Custom keyboard event trigger to support the Valve selection in the Maintenance -> Manual -> LowLevel page.
    /// Used in the LowLevelView.xaml and calls into the LowLevelViewModel.
    /// </summary>
    public class SelectionEventTrigger : EventTrigger
    {

        public SelectionEventTrigger() : base("KeyDown")
        {
        }

        /// <summary>
        /// If the user presses the &lt;Enter&gt; or &lt;Space&gt; then select the valve that has focus.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void OnEvent(EventArgs eventArgs)
        {
            if (eventArgs is KeyEventArgs e && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                if (e.Source is Path source && source.DataContext is LowLevelViewModel lowLevelViewModel &&
                    source.Parent is Grid parent && ! string.IsNullOrWhiteSpace(parent.Name))
                {
                    var valvePosition = ValvePositionMap.ValveNameToValvePosition(parent.Name);
                    lowLevelViewModel.SetValvePort(valvePosition);
                }
            }
        }
    }
}