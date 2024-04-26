using ScoutUtilities.Common;
using System;

namespace ScoutDomains.Common
{
    public class DataGridExpanderColumnHeader : BaseNotifyPropertyChanged
    {
        public int DilutionFactor
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public  string SampleId { get; set; }
        
        public string KnownSize
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string KnownConcentration
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Lot
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DateTime ExpirationDate
        {
            get { return GetProperty<DateTime>(); }
            set { SetProperty(value); }
        }
                                                                                       
        public string PercentCV
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }
      
        public int ImageId
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public double Adjusted
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double AssayValue
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }
        
        public double TotCount
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }
        
        public double Original
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool Validate
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double AvgTotCount
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double AvgOriginal
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double AvgAdjusted
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public bool AvgValidate
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsStatusUpdated
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }
    }
}