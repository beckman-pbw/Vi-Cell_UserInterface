using ScoutDomains;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Views.CommonControls
{
    public partial class SampleResultsGrid : UserControl
    {
        public SampleResultsGrid()
        {
            InitializeComponent();
        }

        public static DependencyProperty SelectedSampleProperty =
            DependencyProperty.Register(nameof(SelectedSample), typeof(SampleRecordDomain), typeof(SampleResultsGrid));

        public SampleRecordDomain SelectedSample
        {
            get { return (SampleRecordDomain) GetValue(SelectedSampleProperty); }
            set { SetValue(SelectedSampleProperty, value); }
        }
    }
}
