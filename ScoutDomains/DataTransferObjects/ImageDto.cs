using System;
using System.Windows.Media.Imaging;

namespace ScoutDomains.DataTransferObjects
{
    public class ImageDto : IDisposable, ICloneable
    {
        public UInt16 Rows { get; set; }
        public UInt16 Cols { get; set; }
        public byte Type { get; set; }
        public UInt32 Step { get; set; }
        public BitmapSource ImageSource { get; set; }

        #region Dispose

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                this.ImageSource = null;
            }
        }

        ~ImageDto()
        {
            this.Dispose(false);
        }

        #endregion

        public object Clone()
        {
            var clone = (ImageDto) MemberwiseClone();
            if (ImageSource != null) clone.ImageSource = ImageSource.Clone();
            return clone;
        }
    }

}