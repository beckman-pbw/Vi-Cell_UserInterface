using ScoutDomains.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Media;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for LineGraphForQualityControl.xaml
    /// </summary>
    public partial class LineGraphForQualityControl
    {
        public LineGraphForQualityControl()
        {
            InitializeComponent();
            InvalidateVisual();
        }

        public bool IsFullScreenVisible
        {
            get { return (bool) GetValue(IsFullScreenVisibleProperty); }
            set { SetValue(IsFullScreenVisibleProperty, value); }
        }

        public bool IsGraphTypeDropdownEnable
        {
            get { return (bool)GetValue(IsGraphTypeDropdownEnableProperty); }
            set { SetValue(IsGraphTypeDropdownEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsGraphTypeDropdownEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsGraphTypeDropdownEnableProperty =
            DependencyProperty.Register("IsGraphTypeDropdownEnable", typeof(bool), typeof(LineGraphForQualityControl), new PropertyMetadata(true));




        // Using a DependencyProperty as the backing store for IsFullScreenVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFullScreenVisibleProperty =
            DependencyProperty.Register("IsFullScreenVisible", typeof(bool), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public KeyValuePair<dynamic, double> SelectedItem
        {
            get { return (KeyValuePair<dynamic, double>) GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(KeyValuePair<dynamic, double>),
                typeof(LineGraphForQualityControl), new PropertyMetadata(null));

        public List<LineGraphDomain> GraphList
        {
            get { return (List<LineGraphDomain>) GetValue(GraphListProperty); }
            set { SetValue(GraphListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GraphList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphListProperty =
            DependencyProperty.Register("GraphList", typeof(List<LineGraphDomain>), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public LineGraphDomain SelectedGraph
        {
            get { return (LineGraphDomain) GetValue(SelectedGraphProperty); }
            set { SetValue(SelectedGraphProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGraph.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGraphProperty =
            DependencyProperty.Register("SelectedGraph", typeof(LineGraphDomain), typeof(LineGraphForQualityControl),
                new PropertyMetadata(SelectedGraphPropertyCallBack));

        private static void SelectedGraphPropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = d as LineGraphForQualityControl;
            var graphViewModelInstance = e.NewValue as LineGraphDomain;
            if (graph != null && graphViewModelInstance != null)
            {
                graph.primeLineSeries.ItemsSource = graph.SelectedGraph.GraphDetailList;
                graph.UpdateAcceptanceLimit(graphViewModelInstance);
            }
        }

        private void UpdateAcceptanceLimit(LineGraphDomain graphViewModelInstance)
        {
            var lowerLimitList = new List<KeyValuePair<dynamic, double>>();
            var upperLimitList = new List<KeyValuePair<dynamic, double>>();
            ((LineSeries) lineChart.Series[2]).DataContext = null;
            ((LineSeries) lineChart.Series[3]).DataContext = null;

            
            if (graphViewModelInstance.AcceptanceLowerLimit != 0 && graphViewModelInstance.AcceptanceUpperLimit != 0)
            {
                foreach (KeyValuePair<dynamic, double> val in graphViewModelInstance.GraphDetailList)
                {
                    lowerLimitList.Add(new KeyValuePair<dynamic, double>(val.Key,
                        graphViewModelInstance.AcceptanceLowerLimit));
                    upperLimitList.Add(new KeyValuePair<dynamic, double>(val.Key,
                        graphViewModelInstance.AcceptanceUpperLimit));
                }

                ((LineSeries) lineChart.Series[2]).DataContext = lowerLimitList;
                ((LineSeries) lineChart.Series[3]).DataContext = upperLimitList;
            }

            //else
            //{
            //    ((LineSeries)lineChart.Series[2])..  = null;
            //    ((LineSeries)lineChart.Series[3]).Visibility = Visibility.Collapsed;
            //}
        }

        public int YAxisMin
        {
            get { return (int) GetValue(YAxisMinProperty); }
            set { SetValue(YAxisMinProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YAxisMin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YAxisMinProperty =
            DependencyProperty.Register("YAxisMin", typeof(int), typeof(LineGraphForQualityControl),
                new PropertyMetadata(0));

        public int YAxisMax
        {
            get { return (int) GetValue(YAxisMaxProperty); }
            set { SetValue(YAxisMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YAxisMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YAxisMaxProperty =
            DependencyProperty.Register("YAxisMax", typeof(int), typeof(LineGraphForQualityControl),
                new PropertyMetadata(0));

        public int YAxisInterval
        {
            get { return (int) GetValue(YAxisIntervalProperty); }
            set { SetValue(YAxisIntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YAxisInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YAxisIntervalProperty =
            DependencyProperty.Register("YAxisInterval", typeof(int), typeof(LineGraphForQualityControl),
                new PropertyMetadata(0));

        public string PrimaryLegendName
        {
            get { return (string) GetValue(PrimaryLegendNameProperty); }
            set { SetValue(PrimaryLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryLegendNameProperty =
            DependencyProperty.Register("PrimaryLegendName", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public string SecondaryLegendName
        {
            get { return (string) GetValue(SecondaryLegendNameProperty); }
            set { SetValue(SecondaryLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryLegendNameProperty =
            DependencyProperty.Register("SecondaryLegendName", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public Brush PlotColor
        {
            get { return (Brush) GetValue(PlotColorProperty); }
            set { SetValue(PlotColorProperty, value); }
        }

        public static readonly DependencyProperty PlotColorProperty =
            DependencyProperty.Register("PlotColor", typeof(Brush), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public string LegendTitle
        {
            get { return (string) GetValue(LegendTitleProperty); }
            set { SetValue(LegendTitleProperty, value); }
        }

        public static readonly DependencyProperty LegendTitleProperty =
            DependencyProperty.Register("LegendTitle", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public string XAxisName
        {
            get { return (string) GetValue(XAxisNameProperty); }
            set { SetValue(XAxisNameProperty, value); }
        }

        public static readonly DependencyProperty XAxisNameProperty =
            DependencyProperty.Register("XAxisName", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public string YAxisName
        {
            get { return (string) GetValue(YAxisNameProperty); }
            set { SetValue(YAxisNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YAxisName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YAxisNameProperty =
            DependencyProperty.Register("YAxisName", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public string GraphName
        {
            get { return (string) GetValue(GraphNameProperty); }
            set { SetValue(GraphNameProperty, value); }
        }

        public static readonly DependencyProperty GraphNameProperty =
            DependencyProperty.Register("GraphName", typeof(string), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public bool IsMultiAxisEnable
        {
            get { return (bool) GetValue(IsMultiAxisEnableProperty); }
            set { SetValue(IsMultiAxisEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMultiAxisEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMultiAxisEnableProperty =
            DependencyProperty.Register("IsMultiAxisEnable", typeof(bool), typeof(LineGraphForQualityControl),
                new PropertyMetadata(false));

        public bool ShowGridLine
        {
            get { return (bool) GetValue(ShowGridLineProperty); }
            set { SetValue(ShowGridLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowGridLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowGridLineProperty =
            DependencyProperty.Register("ShowGridLine", typeof(bool), typeof(LineGraphForQualityControl),
                new PropertyMetadata(true));

        public double AcceptanceLowerLimit
        {
            get { return (double) GetValue(AcceptanceLowerLimitProperty); }
            set { SetValue(AcceptanceLowerLimitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AcceptanceLowerLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcceptanceLowerLimitProperty =
            DependencyProperty.Register("AcceptanceLowerLimit", typeof(double), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));

        public double AcceptanceUpperLimit
        {
            get { return (double) GetValue(AcceptanceUpperLimitProperty); }
            set { SetValue(AcceptanceUpperLimitProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AcceptanceUpperLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcceptanceUpperLimitProperty =
            DependencyProperty.Register("AcceptanceUpperLimit", typeof(double), typeof(LineGraphForQualityControl),
                new PropertyMetadata(null));
    }
}