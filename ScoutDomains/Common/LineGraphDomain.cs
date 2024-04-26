using ScoutUtilities.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ScoutDomains.Common
{
    public class LineGraphDomain : BaseNotifyPropertyChanged
    {
        public ICommand ExpandCommand { get; set; }

        public double AcceptanceLowerLimit
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double AcceptanceUpperLimit
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool IsExpandableView
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string SecondaryTrendLegendName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); } 
        }

        public string PrimaryTrendLegendName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string PrimaryTrendLabel
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string SecondaryTrendLabel
        {
            get { return GetProperty<string>(); }
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

        public string SecondaryLegendName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<KeyValuePair<dynamic, double>> PrimaryTrendPoints
        {
            get { return GetProperty<ObservableCollection<KeyValuePair<dynamic, double>>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<KeyValuePair<dynamic, double>> SecondaryTrendPoints
        {
            get { return GetProperty<ObservableCollection<KeyValuePair<dynamic, double>>>(); }
            set { SetProperty(value); }
        }

        private ObservableCollection<KeyValuePair<dynamic, double>> _graphDetailList;
        public ObservableCollection<KeyValuePair<dynamic, double>> GraphDetailList
        {
            get { return _graphDetailList ?? (_graphDetailList = 
                new ObservableCollection<KeyValuePair<dynamic, double>>()); }
            set
            {
                _graphDetailList = value;
                NotifyPropertyChanged(nameof(GraphDetailList));
            }
        }

        private ObservableCollection<KeyValuePair<dynamic, double>> _multiGraphDetailList;
        public ObservableCollection<KeyValuePair<dynamic, double>> MultiGraphDetailList
        {
            get { return _multiGraphDetailList ?? (_multiGraphDetailList = 
                new ObservableCollection<KeyValuePair<dynamic, double>>()); }
            set
            {
                _multiGraphDetailList = value;
                NotifyPropertyChanged(nameof(MultiGraphDetailList));
            }
        }
    }
}