using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class MessageHubView : Dialog
    {
        public MessageHubView(MessageHubViewModel vm) : base(vm)
        {
            InitializeComponent();
            Deactivated += OnDeactivated;
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            if (!IsVisible)
                return;

            if (DataContext != null && DataContext is MessageHubViewModel vm)
            {
                Owner = null; // This forces the main window to retain focus after closing the Message Hub
                vm.CloseCommand.Execute(this);
            }
            else
            {
                Close();
            }
        }

        private void OnScrollChanged(object sender, RoutedEventArgs e)
        {
            var sv = FindVisualChild<ScrollViewer>(lstMsgs);
            var verticalScrollbarVisibility = sv.ComputedVerticalScrollBarVisibility;
            if (verticalScrollbarVisibility == Visibility.Visible)
            {
                lstMsgs.ItemContainerStyle = FindResource("lvItemWithScrollBar") as Style;
            }
            else
            {
                lstMsgs.ItemContainerStyle = FindResource("lvItemWithoutScrollBar") as Style;
            }
        }

        // Margin Adjustment on Scroll Bar Visibility
        private TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is TChildItem item)
                {
                    return item;
                }

                var childOfChild = FindVisualChild<TChildItem>(child);
                if (childOfChild != null) return childOfChild;
            }

            return null;
        }
    }
}

