using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using ScoutUtilities.Common;

namespace ScoutUI.Common.Controls
{
    public class BaseCarouselControl : Control, INotifyPropertyChanged
    {
        #region Properties & Fields

        public const string CirclePanelButton = "CirclePanelButton";
        public const string CirclePanelLabel = "CirclePanelLabel";

        protected TextBox _txtPosition;

        public StringBuilder DefinedSelectedSample;
        public List<int> ConsecutiveItemList = new List<int>();
        public double SetGridRotateAngle { get; set; }

        #endregion

        #region Overriden Methods

        public override void OnApplyTemplate()
        {
            _txtPosition = Template.FindName("txtPosition", this) as TextBox;
            if (_txtPosition != null)
            {
                _txtPosition.TextChanged += txtPosition_TextChanged;
            }
            base.OnApplyTemplate();
        }

        protected virtual void txtPosition_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        #endregion

        #region INotify Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void NotifyPropertyChanged(string param)
        {
            DispatcherHelper.ApplicationExecute(() =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(param)));
        }

        #endregion
    }
}