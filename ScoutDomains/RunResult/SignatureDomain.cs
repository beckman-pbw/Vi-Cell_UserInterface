using ScoutUtilities.Common;
using ScoutUtilities.Interfaces;
using System;

namespace ScoutDomains
{
    public class SignatureDomain : BaseNotifyPropertyChanged, ISignature, ICloneable
    {
        public SignatureDomain()
        {
            SignatureMeaning = string.Empty;
            SignatureIndicator = string.Empty;
        }

        public SignatureDomain(string meaning, string indicator)
        {
            SignatureMeaning = meaning;
            SignatureIndicator = indicator;
        }

        public string SignatureMeaning
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value == null ? string.Empty : value.Trim()); }
        }

        public string SignatureIndicator
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value == null ? string.Empty : value.Trim()); }
        }

        public object Clone()
        {
            var clone = (SignatureDomain) MemberwiseClone();
            CloneBaseProperties(clone);
            return clone;
        }
    }
}
