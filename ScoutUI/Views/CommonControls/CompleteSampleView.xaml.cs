using System.Windows;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for CompleteSampleView.xaml
    /// </summary>
    public partial class CompleteSampleView
    {
        public CompleteSampleView()
        {
            InitializeComponent();
        }

        public System.Collections.IList SampleViewList
        {
            get { return (System.Collections.IList) GetValue(OrderDetailsProperty); }
            set { SetValue(OrderDetailsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OrderDetails.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrderDetailsProperty =
            DependencyProperty.Register("OrderDetails", typeof(System.Collections.IList), typeof(CompleteSampleView),
                new PropertyMetadata(null));


        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(CompleteSampleView),
                new PropertyMetadata(null));
    }
}