using System.Collections;
using System.Windows;

namespace ScoutUI.Views.CommonControls
{
    public partial class CommonListBox
    {
        public CommonListBox()
        {
            InitializeComponent();
        }

        public IList ItemList
        {
            get { return (IList) GetValue(ItemListProperty); }
            set { SetValue(ItemListProperty, value); }
        }

        public static readonly DependencyProperty ItemListProperty = DependencyProperty.Register(
            nameof(ItemList), typeof(IList), typeof(CommonListBox), new PropertyMetadata(null));


        public object SelectedItem
        {
            get { return (object) GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(CommonListBox), new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ucControl = d as CommonListBox;
            ucControl?.ListViewControl.ScrollIntoView(e.NewValue);
        }

        public bool IsSelectedColor
        {
            get { return (bool) GetValue(IsSelectedColorProperty); }
            set { SetValue(IsSelectedColorProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedColorProperty = DependencyProperty.Register(
            nameof(IsSelectedColor), typeof(bool), typeof(CommonListBox), new PropertyMetadata());
    }
}
