using ScoutDomains.Common;
using ScoutViewModels.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for ScatterGraph.xaml
    /// </summary>
    public partial class ScatterGraph
    {
        public ScatterGraph()
        {
            InitializeComponent();
            InvalidateVisual();
        }

        public bool IsFullScreenVisible
        {
            get { return (bool) GetValue(IsFullScreenVisibleProperty); }
            set { SetValue(IsFullScreenVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenVisibleProperty =
            DependencyProperty.Register("IsFullScreenVisible", typeof(bool), typeof(ScatterGraph),
                new PropertyMetadata(FullScreenVisibleCallBack));

        private static void FullScreenVisibleCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scatterGraph = d as ScatterGraph;
            if (scatterGraph == null)
                return;
            scatterGraph.cmbFraphItem.Visibility = scatterGraph.btnExpand.Visibility =
                scatterGraph.IsFullScreenVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public List<LineGraphDomain> GraphList
        {
            get { return (List<LineGraphDomain>) GetValue(GraphListProperty); }
            set { SetValue(GraphListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GraphList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphListProperty =
            DependencyProperty.Register("GraphList", typeof(List<LineGraphDomain>), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public LineGraphDomain SelectedGraph
        {
            get { return (LineGraphDomain) GetValue(SelectedGraphProperty); }
            set { SetValue(SelectedGraphProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGraph.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGraphProperty =
            DependencyProperty.Register("SelectedGraph", typeof(LineGraphDomain), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public string GraphName
        {
            get { return (string) GetValue(GraphNameProperty); }
            set { SetValue(GraphNameProperty, value); }
        }

        public static readonly DependencyProperty GraphNameProperty =
            DependencyProperty.Register("GraphName", typeof(string), typeof(ScatterGraph), new PropertyMetadata(null));

        public string LegendTitle
        {
            get { return (string) GetValue(LegendTitleProperty); }
            set { SetValue(LegendTitleProperty, value); }
        }

        public static readonly DependencyProperty LegendTitleProperty =
            DependencyProperty.Register("LegendTitle", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public Brush PlotColor
        {
            get { return (Brush) GetValue(PlotColorProperty); }
            set { SetValue(PlotColorProperty, value); }
        }

        public static readonly DependencyProperty PlotColorProperty =
            DependencyProperty.Register("PlotColor", typeof(Brush), typeof(ScatterGraph), new PropertyMetadata(null));

        public string XAxisName
        {
            get { return (string) GetValue(XAxisNameProperty); }
            set { SetValue(XAxisNameProperty, value); }
        }

        public static readonly DependencyProperty XAxisNameProperty =
            DependencyProperty.Register("XAxisName", typeof(string), typeof(ScatterGraph), new PropertyMetadata(null));

        public string YAxisName
        {
            get { return (string) GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }

        public static readonly DependencyProperty YAxisProperty =
            DependencyProperty.Register("YAxisName", typeof(string), typeof(ScatterGraph), new PropertyMetadata(null));

        public List<string> LegendItem
        {
            get { return (List<string>) GetValue(LegendItemProperty); }
            set { SetValue(LegendItemProperty, value); }
        }

        public static readonly DependencyProperty LegendItemProperty =
            DependencyProperty.Register("LegendItem", typeof(List<string>), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public string PrimaryLegendName
        {
            get { return (string) GetValue(PrimaryLegendNameProperty); }
            set { SetValue(PrimaryLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryLegendNameProperty =
            DependencyProperty.Register("PrimaryLegendName", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public string SecondaryLegendName
        {
            get { return (string) GetValue(SecondaryLegendNameProperty); }
            set { SetValue(SecondaryLegendNameProperty, value); }
        }

        public string PrimaryTrendLegendName
        {
            get { return (string) GetValue(PrimaryTrendLegendNameProperty); }
            set { SetValue(PrimaryTrendLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryTrendLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryTrendLegendNameProperty =
            DependencyProperty.Register("PrimaryTrendLegendName", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public string SecondaryTrendLegendName
        {
            get { return (string) GetValue(SecondaryTrendLegendNameProperty); }
            set { SetValue(SecondaryTrendLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryTrendLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryTrendLegendNameProperty =
            DependencyProperty.Register("SecondaryTrendLegendName", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for SecondaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryLegendNameProperty =
            DependencyProperty.Register("SecondaryLegendName", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public bool IsMultiAxisEnable
        {
            get { return (bool) GetValue(IsMultiAxisEnableProperty); }
            set { SetValue(IsMultiAxisEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMultiAxisEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMultiAxisEnableProperty =
            DependencyProperty.Register("IsMultiAxisEnable", typeof(bool), typeof(ScatterGraph),
                new PropertyMetadata(false));

        public List<KeyValuePair<dynamic, double>> PrimaryTrendPoints
        {
            get { return (List<KeyValuePair<dynamic, double>>) GetValue(PrimaryTrendPointsProperty); }
            set { SetValue(PrimaryTrendPointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryTrendPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryTrendPointsProperty =
            DependencyProperty.Register("PrimaryTrendPoints", typeof(List<KeyValuePair<dynamic, double>>),
                typeof(ScatterGraph), new PropertyMetadata(null));

        public List<KeyValuePair<dynamic, double>> SecondaryTrendPoints
        {
            get { return (List<KeyValuePair<dynamic, double>>) GetValue(SecondaryTrendPointsProperty); }
            set { SetValue(SecondaryTrendPointsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryTrendPoints.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryTrendPointsProperty =
            DependencyProperty.Register("SecondaryTrendPoints", typeof(List<KeyValuePair<dynamic, double>>),
                typeof(ScatterGraph), new PropertyMetadata(null));

        public string PrimaryTrendLabel
        {
            get { return (string) GetValue(PrimaryTrendLabelProperty); }
            set { SetValue(PrimaryTrendLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryTrendLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryTrendLabelProperty =
            DependencyProperty.Register("PrimaryTrendLabel", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public string SecondaryTrendLabel
        {
            get { return (string) GetValue(SecondaryTrendLabelProperty); }
            set { SetValue(SecondaryTrendLabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryTrendLabel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryTrendLabelProperty =
            DependencyProperty.Register("SecondaryTrendLabel", typeof(string), typeof(ScatterGraph),
                new PropertyMetadata(null));

        public bool ShowGrid
        {
            get { return (bool) GetValue(ShowGridProperty); }
            set { SetValue(ShowGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register("ShowGrid", typeof(bool), typeof(ScatterGraph), new PropertyMetadata(true));

        public bool IsExpandableView
        {
            get { return (bool) GetValue(IsExpandableViewProperty); }
            set { SetValue(IsExpandableViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpandableView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandableViewProperty =
            DependencyProperty.Register("IsExpandableView", typeof(bool), typeof(ScatterGraph),
                new PropertyMetadata(false));


        public ICommand ExpandCommand
        {
            get { return (ICommand) GetValue(ExpandCommandProperty); }
            set { SetValue(ExpandCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandCommandProperty =
            DependencyProperty.Register("ExpandCommand", typeof(ICommand), typeof(ScatterGraph),
                new PropertyMetadata(null));
    }
}