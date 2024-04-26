using System;
using ScoutUtilities.Common;
using ScoutUtilities.Structs;

namespace ScoutDomains
{
    public class BasicResultDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public E_ERRORCODE ProcessedStatus
        {
            get { return GetProperty<E_ERRORCODE>(); }
            set { SetProperty(value); }
        }

        public uint TotalCumulativeImage
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public uint TotalCells
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public uint ViableCells
        {
            get { return GetProperty<uint>(); }
            set { SetProperty(value); }
        }

        public double ConcentrationML
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        private double _viableConcentration;

        public double ViableConcentration
        {
            get { return _viableConcentration; }
            set
            {
                _viableConcentration = value;
                if (_viableConcentration < 1.15 && _viableConcentration > 2.19)
                    IsViableConcentrationValid = true;
                else
                    IsViableConcentrationValid = false;
                NotifyPropertyChanged(nameof(ViableConcentration));
            }
        }

        public double Viability
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Size
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double ViableSize
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Circularity
        {
            get { return GetProperty<double>(); }
            set
            {
                if (value > 1.0)
                    throw new ArgumentOutOfRangeException("Circularity cannot be greater than 1");
                SetProperty(value);
            }
        }

        public double ViableCircularity
        {
            get { return GetProperty<double>(); }
            set
            {
                if (value > 1.0)
                    throw new ArgumentOutOfRangeException("Circularity cannot be greater than 1");
                SetProperty(value);
            }
        }

        public bool IsConcentrationValid
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsViableConcentrationValid
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public double AverageCellsPerImage
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public uint Bubble { get; set; }

        public uint AvgBackground { get; set; }

        public uint ClusterCount { get; set; }

        public object Clone()
        {
            var clone = (BasicResultDomain) MemberwiseClone();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}