using ScoutUtilities.Common;
using ScoutUtilities.Events;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Views._1___Home
{
    public partial class SampleSetControl : UserControl
    {
        public const int DATA_GRID_WITH_IMAGE_WIDTH = 490;
        public const int DEFAULT_SAMPLE_SET_WIDTH = 1230;
        public const int DEFAULT_SAMPLE_SET_WIDTH_SUBTRACTION = 50;
        public const int DEFAULT_WINDOW_WIDTH_SUBTRACTION = 60;

        private Subscription<Notification<bool>> _imageToggleMessagesSubscriber;

        public SampleSetControl()
        {
            InitializeComponent();
            Loaded += SampleSetControl_Loaded;
            Unloaded += SampleSetControl_Unloaded;
            _imageToggleMessagesSubscriber = MessageBus.Default.Subscribe<Notification<bool>>(HandleImageToggleMessages);
        }

        private void HandleImageToggleMessages(Notification<bool> msg)
        {
            if (msg.Token.Equals(MessageToken.ShowRunningImagesToggleButtonToken))
            {
                DispatcherHelper.ApplicationExecute(() => 
                    ShowRunningImagesToggleButton_OnClick(this, new RoutedEventArgs()));
            }
        }

        private double GetAvailableWidth()
        {
            // using the window width to fix bug PC3549-2422
            var window = Window.GetWindow(SampleSetUserControl);
            if (window != null)
            {
                var winWidth = window.ActualWidth;
                return winWidth - DEFAULT_WINDOW_WIDTH_SUBTRACTION;
            }

            var width = SampleSetUserControl.ActualWidth <= 1
                ? DEFAULT_SAMPLE_SET_WIDTH
                : SampleSetUserControl.ActualWidth - DEFAULT_SAMPLE_SET_WIDTH_SUBTRACTION;

            return Math.Max(width, DATA_GRID_WITH_IMAGE_WIDTH);
        }

        private void SetWidth()
        {
            // Make the datagrid large or small depending on whether the Images are being shown or not
            var availableWidth = GetAvailableWidth();

            if (ShowRunningImagesToggleButton.IsChecked == true)
            {
                var width = GetAvailableWidth() - Math.Max(SampleSetImageGrid.ActualWidth, SampleSetImageGrid.Width) - 5;
                SampleSetDataGrid.Width = Math.Max(width, DATA_GRID_WITH_IMAGE_WIDTH);
            }
            else
            {
                SampleSetDataGrid.Width = availableWidth;
            }
        }

        #region Event Handlers

        private void SampleSetControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWidth();
        }

        private void SampleSetControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MessageBus.Default.UnSubscribe(ref _imageToggleMessagesSubscriber);
        }

        private void ShowRunningImagesToggleButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWidth();
        }

        private void Expander_OnExpanded(object sender, RoutedEventArgs e)
        {
            SetWidth();
        }

        #endregion
    }
}
