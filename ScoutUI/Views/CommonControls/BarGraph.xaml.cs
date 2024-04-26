// ***********************************************************************
// <copyright file="BarGraph.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using ScoutDomains.Common;
using ScoutUtilities;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.DataVisualization;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for BarGraph.xaml
    /// </summary>
    public partial class BarGraph
    {
        public BarGraph()
        {
            InitializeComponent();
            InvalidateVisual();
            LineDrawer = new LineDrawer();
        }

        #region Properties

        const int DecimalPlaces = 2;

        public LineDrawer LineDrawer { get; set; }

        public bool IsExpandView { get; set; }


        private double _lowerRange;

        public double LowerRange
        {
            get { return _lowerRange; }
            set { _lowerRange = Math.Round(value, DecimalPlaces); }
        }

        private double _upperRange;

        public double UpperRange
        {
            get { return _upperRange; }
            set { _upperRange = Math.Round(value, DecimalPlaces); }
        }

        private double _number;

        public double Number
        {
            get { return _number; }
            set { _number = Math.Round(value, DecimalPlaces); }
        }

        private double _percentage;

        public double Percentage
        {
            get { return _percentage; }
            set { _percentage = Math.Round(value, 1); }
        }

        private double _mean;

        public double Mean
        {
            get { return _mean; }
            set { _mean = Math.Round(value, DecimalPlaces); }
        }

        private double _standardDeviation;

        public double StandardDeviation
        {
            get { return _standardDeviation; }
            set { _standardDeviation = Math.Round(value, DecimalPlaces); }
        }

        private double _mode;

        public double Mode
        {
            get { return _mode; }
            set { _mode = Math.Round(value, DecimalPlaces); }
        }

        public bool IsDistributionGraphEnable { get; set; }

        #endregion

        #region Dependency Properties

        public GraphType SelectedGraphType
        {
            get { return (GraphType)GetValue(SelectedGraphTypeProperty); }
            set { SetValue(SelectedGraphTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGraphTypeProperty =
            DependencyProperty.Register("SelectedGraphType", typeof(GraphType), typeof(BarGraph),
                new PropertyMetadata(GraphType.SizeDistribution));

        public List<BarGraphDomain> GraphList
        {
            get { return (List<BarGraphDomain>)GetValue(GraphListProperty); }
            set { SetValue(GraphListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GraphList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphListProperty =
            DependencyProperty.Register("GraphList", typeof(List<BarGraphDomain>), typeof(BarGraph),
                new PropertyMetadata(null));

        public BarGraphDomain SelectedGraph
        {
            get { return (BarGraphDomain)GetValue(SelectedGraphProperty); }
            set { SetValue(SelectedGraphProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedGraph.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedGraphProperty =
            DependencyProperty.Register("SelectedGraph", typeof(BarGraphDomain), typeof(BarGraph),
                new PropertyMetadata(SelectedGraphPropertyCallBack));

        public string GraphName
        {
            get { return (string)GetValue(GraphNameProperty); }
            set { SetValue(GraphNameProperty, value); }
        }

        public static readonly DependencyProperty GraphNameProperty =
            DependencyProperty.Register("GraphName", typeof(string), typeof(BarGraph), new PropertyMetadata(null));

        public string LegendTitle
        {
            get { return (string)GetValue(LegendTitleProperty); }
            set { SetValue(LegendTitleProperty, value); }
        }

        public static readonly DependencyProperty LegendTitleProperty =
            DependencyProperty.Register("LegendTitle", typeof(string), typeof(BarGraph), new PropertyMetadata(null));


        public string XAxisName
        {
            get { return (string)GetValue(XAxisNameProperty); }
            set { SetValue(XAxisNameProperty, value); }
        }

        public static readonly DependencyProperty XAxisNameProperty = 
            DependencyProperty.Register("XAxisName", typeof(string), typeof(BarGraph), new PropertyMetadata(null));

        public string YAxisName
        {
            get { return (string)GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }

        public static readonly DependencyProperty YAxisProperty =
            DependencyProperty.Register("YAxisName", typeof(string), typeof(BarGraph), new PropertyMetadata(null));

        public ObservableCollection<KeyValuePair<string,string>> LegendItem
        {
            get { return (ObservableCollection<KeyValuePair<string, string>>)GetValue(LegendItemProperty); }
            set { SetValue(LegendItemProperty, value); }
        }

        public static readonly DependencyProperty LegendItemProperty =
            DependencyProperty.Register("LegendItem", typeof(ObservableCollection<KeyValuePair<string, string>>), typeof(BarGraph),
                new PropertyMetadata(null));

        public string PrimaryLegendName
        {
            get { return (string)GetValue(PrimaryLegendNameProperty); }
            set { SetValue(PrimaryLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrimaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrimaryLegendNameProperty =
            DependencyProperty.Register("PrimaryLegendName", typeof(string), typeof(BarGraph),
                new PropertyMetadata(null));

        public string SecondaryLegendName
        {
            get { return (string)GetValue(SecondaryLegendNameProperty); }
            set { SetValue(SecondaryLegendNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryLegendName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryLegendNameProperty =
            DependencyProperty.Register("SecondaryLegendName", typeof(string), typeof(BarGraph),
                new PropertyMetadata(null));

        public bool IsMultiAxisEnable
        {
            get { return (bool)GetValue(IsMultiAxisEnableProperty); }
            set { SetValue(IsMultiAxisEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMultiAxisEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMultiAxisEnableProperty =
            DependencyProperty.Register("IsMultiAxisEnable", typeof(bool), typeof(BarGraph),
                new PropertyMetadata(MultiAxisEnablePropertyCallBack));

        public bool IsFullScreenVisible
        {
            get { return (bool)GetValue(IsFullScreenVisibleProperty); }
            set { SetValue(IsFullScreenVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFullScreenVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFullScreenVisibleProperty =
            DependencyProperty.Register("IsFullScreenVisible", typeof(bool), typeof(BarGraph),
                new PropertyMetadata(FullScreenVisibleCallBack));

        //Expandable view and graph with multi axis.
        public bool IsExpandableMultiAxis
        {
            get { return (bool)GetValue(IsExpandableMultiAxisProperty); }
            set { SetValue(IsExpandableMultiAxisProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpandableMultiAxis.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandableMultiAxisProperty =
            DependencyProperty.Register("IsExpandableMultiAxis", typeof(bool), typeof(BarGraph),
                new PropertyMetadata(false));


        //Expandable view and graph with subrage statistics
        public bool IsExpandableSubRangeStat
        {
            get { return (bool)GetValue(IsExpandableSubRangeStatProperty); }
            set { SetValue(IsExpandableSubRangeStatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsExpandableSubRangeStat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsExpandableSubRangeStatProperty =
            DependencyProperty.Register("IsExpandableSubRangeStat", typeof(bool), typeof(BarGraph), 
                new PropertyMetadata(false));


        public int Interval
        {
            get { return (int)GetValue(IntervalProperty); }
            set { SetValue(IntervalProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Interval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.Register("Interval", typeof(int), typeof(BarGraph), new PropertyMetadata(1));


        public bool IsSetFocusEnable
        {
            get { return (bool)GetValue(IsSetFocusEnableProperty); }
            set { SetValue(IsSetFocusEnableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSetFocusEnable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSetFocusEnableProperty =
            DependencyProperty.Register("IsSetFocusEnable", typeof(bool), typeof(BarGraph), new PropertyMetadata(false));

        #endregion

        #region Dependency Property Callback

        private static void MultiAxisEnablePropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphInstance = d as BarGraph;
            graphInstance?.UpdateExpandableMultiAxisProperty();
        }

        private static void SelectedGraphPropertyCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphInstance = d as BarGraph;
            var newGraphInstance = e.NewValue as BarGraphDomain;
            if (graphInstance == null || newGraphInstance == null)
                return;
            graphInstance.LineDrawer.ResetLines();
            graphInstance.BarGraphColumnSeries.ItemsSource = new List<KeyValuePair<dynamic, double>>();
            var graphType = newGraphInstance.SelectedGraphType;
            graphInstance.BarGraphColumnSeries.ItemsSource = newGraphInstance.PrimaryGraphDetailList;
            graphInstance.ClearSecondaryColumnSeries();
            graphInstance.ClearLineMarker();
            graphInstance.UpdateShowSecondLineMarker(graphType);
            graphInstance.CreateColumnSeries();
            graphInstance.CalculateStatistics(graphType,"-");
            graphInstance.xAxis.Title = newGraphInstance.XAxisName;
            graphInstance.yAxis.Title = newGraphInstance.YAxisName;
            graphInstance.BarGraphColumnSeries.Title = newGraphInstance.PrimaryLegendName;
            var style = new Style(typeof(ColumnDataPoint));
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Green)));
            style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Colors.Green)));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(100)));
            graphInstance.BarGraphColumnSeries.DataPointStyle = style;
            if (graphInstance.IsSetFocusEnable)
                graphInstance.barChart.Title = newGraphInstance.GraphName;
            if(graphInstance.IsFullScreenVisible && !graphInstance.IsSetFocusEnable)
                graphInstance.IsExpandView = true;
        }

        #endregion

        #region Private Methods

        private void UpdateExpandableMultiAxisProperty()
        {
            IsExpandableMultiAxis = IsMultiAxisEnable && IsFullScreenVisible;
        }

        private void UpdateShowSecondLineMarker(GraphType graphType)
        {
            switch (graphType)
            {
                case GraphType.SizeDistribution:
                case GraphType.ViableSizeDistribution:
                case GraphType.CircularityDistribution:
                case GraphType.ViableCircularityDistribution:
                    if (IsFullScreenVisible)
                    {
                        IsExpandableSubRangeStat = true;
                    }
                    IsDistributionGraphEnable = true;
                    break;
                default:
                    if (IsFullScreenVisible)
                    {
                        IsExpandableSubRangeStat = true;
                    }
                    IsDistributionGraphEnable = false;
                    break;
            }
        }

        private void CalculateStatistics(GraphType graphType,string noData=null)
        {
            switch (graphType)
            {
                case GraphType.SizeDistribution:
                case GraphType.ViableSizeDistribution:
                    CalculateSubrangeStatistics();
                    UpdateSubrangeStatistics(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_MicroMeter_UnitWithOutBracket"));
                    break;

                case GraphType.CircularityDistribution:
                case GraphType.ViableCircularityDistribution:
                    CalculateSubrangeStatistics();
                    UpdateSubrangeStatistics(string.Empty);
                    break;

                case GraphType.ClusterSizeDistribution:
                    UpdateClusterStatistics(noData);
                    break;

                case GraphType.Viability:
                    UpdateViabilityStatistics(noData);
                    break;

                case GraphType.TotalCells:
                    UpdateCellCountStatistics(noData);
                    break;

                case GraphType.Concentration:
                    UpdateConcentrationStatistics(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_TenthPower"), noData);
                    break;

                case GraphType.AverageSize:
                    UpdateAvgDiameterStatistics(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_MicroMeter_UnitWithOutBracket"),noData);
                    break;

                case GraphType.AverageCircularity:
                    UpdateAvgCircularityStatistics(noData);
                    break;
                case GraphType.AverageBackground:
                    UpdateAvgBackgroundStatistics(noData);
                    break;
                case GraphType.Bubble:
                    UpdateBubbleCountStatistics(noData);
                    break;
            }
        }

        /// <summary>
        /// Calculate the subrange statics based on user range selection.
        /// </summary>
        private void CalculateSubrangeStatistics()
        {
            if (SelectedGraph?.PrimaryGraphDetailList?.Count > 0)
            {
                // We *always* use the full data set.
                UpperRange = SelectedGraph.PrimaryGraphDetailList.Select(x => x.Key).Max();
                LowerRange = SelectedGraph.PrimaryGraphDetailList.Select(x => x.Key).Min();
                Percentage = 100;

                SelectedGraph.CalculateHistogramStatistics();

                Number = SelectedGraph.Number;
                Mean = SelectedGraph.Mean;
                StandardDeviation = SelectedGraph.StandardDeviation;
                Mode = SelectedGraph.Mode;
            }
            else
            {
                Number = 0;
                Percentage = 0;
                Mean = 0;
                StandardDeviation = 0;
                Mode = 0;
            }
        }

        /// <summary>
        /// Updates the subrange statistics.
        /// </summary>
        private void UpdateSubrangeStatistics(string unit)
        {

            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Range"), Misc.UpdateTrailingPoint(LowerRange, TrailingPoint.Two) +
                                                       " - " +
                                                       Misc.UpdateTrailingPoint(UpperRange, TrailingPoint.Two) + " " +
                                                                                                           unit),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count"), Misc.ConvertToString(Number)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Count") + " %", ChangeDisplayFormat(SubRangeStatistics.Percentage,Percentage, TrailingPoint.One) + " %"),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Mean"),ChangeDisplayFormat(SubRangeStatistics.Mean,Mean, TrailingPoint.Two)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_StandardDeviation"),ChangeDisplayFormat(SubRangeStatistics.StandardDeviation,StandardDeviation, TrailingPoint.Two) + " " + unit),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Mode"),ChangeDisplayFormat(SubRangeStatistics.Mode, Mode, TrailingPoint.Two,unit))
            };
        }

        private string ChangeDisplayFormat(SubRangeStatistics type, double number, TrailingPoint point, string unit=null)
        {
            var changeNumber = Misc.UpdateTrailingPoint(number, point);
            switch (type)
            {
                case SubRangeStatistics.Percentage:
                case SubRangeStatistics.StandardDeviation:
                case SubRangeStatistics.Mean:
                    if (Double.IsNaN(number))
                        changeNumber = Misc.UpdateTrailingPoint(0, point);
                    break;
                case SubRangeStatistics.Mode:
                    if (Double.IsNaN(number) || number == 0)
                    {
                        return ScoutLanguageResources.LanguageResourceHelper.Get("LID_RadioButton_NoneReagent");
                    }

                    return changeNumber + " " + unit;
            }
            return changeNumber;
        }

        private void UpdateClusterStatistics(string noData)
        {
            if (SelectedGraph == null)
                return;
            var count = SelectedGraph.PrimaryGraphDetailList.Find(t => t.Key.Equals(LowerRange)).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(LowerRange + " " + ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_CellClusters"),
                    SetValueForZero(Misc.ConvertToString(count),noData))
            };
        }

        private void UpdateViabilityStatistics(string noData)
        {
            if (SelectedGraph?.PrimaryGraphDetailList == null)
                return;
            var lowerRange = (int)Math.Floor(LowerRange);
            var viability = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                    SetValueForZero(Misc.ConvertToString(lowerRange),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_TabItem_Viability"),
                    SetValueForZero(viability + " %",noData))

            };
        }

        private void UpdateCellCountStatistics(string noData)
        {
            if (SelectedGraph?.MultiGraphDetailList == null || SelectedGraph.MultiGraphDetailList.Count == 0)
                return;
            var lowerRange = (int) Math.Floor(LowerRange);
            var viabilityCount = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            var nonviableCount  = SelectedGraph.MultiGraphDetailList[0].Find(x => x.Key == lowerRange).Value;
            var totalCount = SelectedGraph.MultiGraphDetailList[1].Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                    SetValueForZero(Misc.ConvertToString(lowerRange),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Totalcells"),
                    SetValueForZero(Misc.ConvertToString(totalCount),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ViaCount"),
                    SetValueForZero(Misc.ConvertToString(viabilityCount),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_NonViaCount"),
                    SetValueForZero(Misc.ConvertToString(nonviableCount),noData))
            };
        }

        private void UpdateConcentrationStatistics(string unit, string noData)
        {
            if (SelectedGraph?.MultiGraphDetailList == null || SelectedGraph.MultiGraphDetailList.Count == 0)
                return;
            var lowerRange = (int)Math.Floor(LowerRange);
            var viabilityConc = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            var nonviableConc = SelectedGraph.MultiGraphDetailList[0].Find(x => x.Key == lowerRange).Value;
            var totalConc = SelectedGraph.MultiGraphDetailList[1].Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                    SetValueForZero(Misc.ConvertToConcString(Math.Floor(LowerRange)),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_TotalCellConcentration"),
                    SetValueForZero(Misc.ConvertToConcString(totalConc)+" " + unit,noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_ViaConc"),
                    SetValueForZero(Misc.ConvertToConcString(viabilityConc)+" " + unit,noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_NonViaConc"),
                    SetValueForZero(Misc.ConvertToConcString(nonviableConc)+" " + unit,noData))
            };
        }

        private void UpdateAvgBackgroundStatistics(string noData)
        {
            if (SelectedGraph?.PrimaryGraphDetailList == null)
                return;
            var lowerRange = Math.Floor(LowerRange);
            var avgBackgroundCount = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                    SetValueForZero(Misc.ConvertToString(lowerRange),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Averagebackground"),
                    SetValueForZero(Misc.ConvertToString(avgBackgroundCount),noData))
            };
        }

        private void UpdateBubbleCountStatistics(string noData)
        {
            if (SelectedGraph?.PrimaryGraphDetailList == null)
                return;
            var lowerRange = Math.Floor(LowerRange);
            var bubbleCount = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image"),
                    SetValueForZero(Misc.ConvertToString(lowerRange),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Result_Bubbles"),
                    SetValueForZero(Misc.ConvertToString(bubbleCount),noData))
            };
        }

        /// <summary>
        /// Calculate image statistics for avg diameter, avg circularity and Background intensity
        /// </summary>
        private void UpdateAvgDiameterStatistics(string unit,string noData)
        {
            var lowerRange = (int)Math.Floor(LowerRange);
            var avgDiameter = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image") ,
                    SetValueForZero(Misc.ConvertToString(Math.Floor(LowerRange)),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_AvgDiameter") ,
                    SetValueForZero(Misc.ConvertToString(avgDiameter)+" " + unit,noData))
            };
        }

        private void UpdateAvgCircularityStatistics(string noData)
        {
            var lowerRange = (int)Math.Floor(LowerRange);
            var AvgCircularity = SelectedGraph.PrimaryGraphDetailList.Find(x => x.Key == lowerRange).Value;
            LegendItem = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_GraphLabel_Image") ,
                    SetValueForZero(Misc.ConvertToString(Math.Floor(LowerRange)),noData)),
                new KeyValuePair<string, string>(ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_AverageCircularity") ,
                    SetValueForZero(Misc.ConvertToString(AvgCircularity),noData))
            };
        }

        private string SetValueForZero(string statisticVal,string noData)
        {
           return (noData == null) ? statisticVal : "-";
        }

        /// <summary>
        /// Clears the secondary column series.
        /// </summary>
        private void ClearSecondaryColumnSeries()
        {
            while (barChart.Series != null && barChart.Series.Count > 1)
            {
                barChart.Series.RemoveAt(barChart.Series.Count - 1);
            }
        }

        /// <summary>
        /// Creates the column series.
        /// </summary>
        private void CreateColumnSeries()
        {
            if (SelectedGraph?.MultiGraphDetailList == null
                ||SelectedGraph.MultiGraphDetailList.Count == 0)
                return;
            // CURRENTLY we only graph *ONE* "multigraph" series, even though we have data
            // for two series.
            var columnSeries = new ColumnSeries();
            var style = new Style(typeof(ColumnDataPoint));
            columnSeries.IndependentValueBinding = new System.Windows.Data.Binding("Key");
            columnSeries.DependentValueBinding = new System.Windows.Data.Binding("Value");
            columnSeries.Title = SelectedGraph.MultiLegendNames[0];
            columnSeries.ItemsSource = SelectedGraph.MultiGraphDetailList[0];
            columnSeries.MouseEnter += OnMouseEnter;
            columnSeries.TouchEnter += OnTouchEnter;
            columnSeries.LegendItemStyle = (Style)BarGraphGrid.FindResource("LegendItemStyle");
            style.Setters.Add(new Setter(BackgroundProperty, new SolidColorBrush(Colors.Red)));
            style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Colors.Red)));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(100)));
            columnSeries.DataPointStyle = style;
            barChart.Series.Add(columnSeries);
        }

        /// <summary>
        /// Fulls the screen visible call back.PrimaryLegendName
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void FullScreenVisibleCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var barGraph = d as BarGraph;
            if (barGraph == null)
                return;
            barGraph.cmbGraphItem.Visibility = barGraph.btnExpand.Visibility =
                barGraph.IsFullScreenVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Events

        private void ClearLineMarker()
        {
            if (LineDrawer != null)
            {
                LineDrawer.UnShowLines(BarGraphGrid, LineDrawer.FirstLine);
                LowerRange = 0;
                UpperRange = 0;
            }
        }

        private void OnTouchEnter(object sender, TouchEventArgs e)
        {
            TouchPoint coordinate = e.GetTouchPoint((Series)barChart.Series[0]);
            SetSelection(e.GetTouchPoint(BarGraphGrid).Position.X, coordinate.Position.X);
        }

        public ICommand ExpandCommand
        {
            get { return (ICommand)GetValue(ExpandCommandProperty); }
            set { SetValue(ExpandCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandCommandProperty =
            DependencyProperty.Register("ExpandCommand", typeof(ICommand), typeof(BarGraph),
                new PropertyMetadata(null));

        #endregion

        private void SetSelection(double xAxisVal,double xPoint)
        {
            if (IsSetFocusEnable)
                return;
            if (!IsDistributionGraphEnable && IsFullScreenVisible)
            {
                IRangeAxis rangeAxis = (LinearAxis)barChart.Axes[1];
                var xAxisValue = rangeAxis.GetValueAtPosition(new UnitValue(xPoint, Unit.Pixels));
                LowerRange = Convert.ToInt32(xAxisValue);
                bool canLineDisplayed;
                LineDrawer.FirstLine = LineDrawer.DrawLine(this, BarGraphGrid, BarGraphColumnSeries,
                    LineDrawer.FirstLine,
                    true, xAxisVal, out canLineDisplayed);
                LineDrawer.ShowLines(BarGraphGrid, LineDrawer.FirstLine);
                IsExpandableSubRangeStat = true;
                CalculateStatistics(SelectedGraph.SelectedGraphType);
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var coordinate = e.GetPosition((Series)barChart.Series[0]);
            SetSelection(e.GetPosition(BarGraphGrid).X, coordinate.X);
        }

    }
 
}