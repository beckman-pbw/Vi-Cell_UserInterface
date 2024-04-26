using ScoutDomains.DataTransferObjects;
using ScoutUtilities.Common;
using ScoutUtilities.Structs;
using System;

namespace ScoutDomains
{
    public class SampleImageRecordDomain : BaseNotifyPropertyChanged, IEquatable<SampleImageRecordDomain>, ICloneable
    {
        public uuidDLL UUID { get; set; }

        public string UserId { get; set; }

        public ulong TimeStamp { get; set; }

        public uint SequenceNumber { get; set; }

        public uuidDLL BrightFieldId { get; set; }

        public int ImageID
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int TotalCumulativeImage
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public ImageSetDto ImageSet { get; set; }

        public BasicResultDomain ResultPerImage
        {
            get { return GetProperty<BasicResultDomain>(); }
            set { SetProperty(value); }
        }

        public object Clone()
        {
            var cloneObj = (SampleImageRecordDomain)MemberwiseClone();
            CloneBaseProperties(cloneObj);

            if (ResultPerImage != null) cloneObj.ResultPerImage = (BasicResultDomain)ResultPerImage.Clone();
            if (ImageSet != null) cloneObj.ImageSet = (ImageSetDto)ImageSet.Clone();
            
            return cloneObj;
        }

        public bool Equals(SampleImageRecordDomain other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return UUID.Equals(other.UUID) &&
                   string.Equals(UserId, other.UserId) &&
                   TimeStamp == other.TimeStamp &&
                   SequenceNumber == other.SequenceNumber &&
                   BrightFieldId.Equals(other.BrightFieldId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((SampleImageRecordDomain) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = UUID.GetHashCode();
                hashCode = (hashCode * 397) ^ (UserId != null ? UserId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TimeStamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) SequenceNumber;
                hashCode = (hashCode * 397) ^ BrightFieldId.GetHashCode();
                return hashCode;
            }
        }
    }
}
