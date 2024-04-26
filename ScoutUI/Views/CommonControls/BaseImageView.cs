using ScoutUI.Common;
using ScoutUtilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using ScoutUtilities.Common;

namespace ScoutUI.Views.CommonControls
{
    public class BaseImageView : BaseUserControl
    {
        #region Properties & Fields

        protected const double ImageScrollViewerHeight = 610;
        protected const double ImageScrollViewerWidth = 1200;
        protected const double ParentControlHeightAndWidth = 2048;
        protected const double HistogramControlHeight = 550;
        protected const double HistogramControlWidth = 1000;
        protected const double ControlDefaultHeightAndWidth = 585;

        public string MenuSelectionName = ApplicationConstants.ImageViewRightClickMenuImageFitSize;

        public bool IsValidate
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private ICommand _imageTraversalCommand;
        public ICommand ImageTraversalCommand => _imageTraversalCommand ?? (_imageTraversalCommand = new RelayCommand(Traversal));
        protected virtual void Traversal(object parameter) { }

        private ICommand _onTapImageCommand;
        public ICommand OnTapImageCommand => _onTapImageCommand ?? (_onTapImageCommand = new RelayCommand(OnTapImage));
        protected virtual void OnTapImage() { }

        #endregion

        #region Methods

        protected static void SetChildPathVisibility(StackPanel stackPanel, Visibility visibility)
        {
            foreach (var child in stackPanel.Children)
            {
                if (child is Path path)
                {
                    path.Visibility = visibility;
                }
            }
        }

        #endregion
    }
}