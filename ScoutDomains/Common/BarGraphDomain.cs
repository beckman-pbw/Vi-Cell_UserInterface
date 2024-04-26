using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutDomains.Common
{
    public class BarGraphDomain : BaseNotifyPropertyChanged
    {
        #region private property

        private List<KeyValuePair<dynamic, double>> _primaryGraphDetailList;
        
        private List<List<KeyValuePair<dynamic, double>>> _multiGraphDetailList;
        
        private List<string> _multiLegendNames;
        
        #endregion

        #region public property
        
        public GraphType SelectedGraphType
        {
            get { return GetProperty<GraphType>(); }
            set { SetProperty(value); }
        }
        
        public string GraphName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string LegendTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
        
        public string XAxisName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
        
        public string YAxisName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsMultiAxisEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string PrimaryLegendName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public List<string> MultiLegendNames
        {
            get { return _multiLegendNames ?? (_multiLegendNames = new List<string>()); }
            set
            {
                _multiLegendNames = value;
                NotifyPropertyChanged(nameof(MultiLegendNames));
            }
        }

        public List<KeyValuePair<dynamic, double>> PrimaryGraphDetailList
        {
            get { return _primaryGraphDetailList ?? (_primaryGraphDetailList = new List<KeyValuePair<dynamic, double>>()); }
            set
            {
                _primaryGraphDetailList = value;
                NotifyPropertyChanged(nameof(PrimaryGraphDetailList));
            }
        }

        //todo: Get rid of this
        public List<List<KeyValuePair<dynamic, double>>> MultiGraphDetailList
        {
            get
            {
                return _multiGraphDetailList ??
                       (_multiGraphDetailList = new List<List<KeyValuePair<dynamic, double>>>());
            }
            set
            {
                _multiGraphDetailList = value;
                NotifyPropertyChanged(nameof(MultiGraphDetailList));
            }
        }

        public bool ShowGridLine
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double Number { get; private set; }

        public double Mean { get; private set; }

        public double StandardDeviation { get; private set; }

        public double Mode { get; private set; }

        #endregion

        public void CalculateHistogramStatistics()
        {
            CalculateNumber();
            CalculateMean();
            CalculateStandardDeviation();
            CalculateMode();
        }

        #region Histogram statistics

        private void CalculateNumber()
        {
            Number = PrimaryGraphDetailList.Sum(x => x.Value);
        }

        private void CalculateMean()
        {
            Mean = PrimaryGraphDetailList.Count == 0
                ? 0.0
                : PrimaryGraphDetailList.Sum(x => (double)x.Key * x.Value) / Number;
        }

        private void CalculateStandardDeviation()
        {
            StandardDeviation = PrimaryGraphDetailList.Count == 0
                ? 0.0
                : Math.Sqrt(PrimaryGraphDetailList.Sum(x => x.Value * Math.Pow((double)x.Key - Mean, 2)) / Number);
        }

        private void CalculateMode()
        {
            Mode = PrimaryGraphDetailList.Count == 0
                ? 0.0
                : (double)PrimaryGraphDetailList.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }

        #endregion

    }
}