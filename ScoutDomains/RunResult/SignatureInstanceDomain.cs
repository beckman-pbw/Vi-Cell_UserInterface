using ScoutUtilities.Common;
using System;

namespace ScoutDomains.RunResult
{
    public class SignatureInstanceDomain : BaseNotifyPropertyChanged, ICloneable
    {
        public SignatureInstanceDomain()
        {
            Signature = new SignatureDomain();
        }

        public DateTime SignedDate { get; set; }

        public SignatureDomain Signature
        {
            get { return GetProperty<SignatureDomain>(); }
            set { SetProperty(value); }
        }

        public string SigningUser
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var clone = (SignatureInstanceDomain) MemberwiseClone();
            CloneBaseProperties(clone);
            if (Signature != null) clone.Signature = (SignatureDomain) Signature.Clone();
            return clone;
        }
    }
}