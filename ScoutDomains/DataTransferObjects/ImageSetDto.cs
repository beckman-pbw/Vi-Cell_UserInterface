using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutDomains.DataTransferObjects
{
    public class ImageSetDto : IDisposable, ICloneable
    {
        public ImageDto BrightfieldImage { get; set; }
        public List<ImageDto> FlourescenceImages { get; set; }

        #region Dispose

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                this.BrightfieldImage?.Dispose();
                this.BrightfieldImage = null;
                if (FlourescenceImages != null)
                {
                    for (int i = FlourescenceImages.Count - 1; i >= 0; i--)
                    {
                        ImageDto item = FlourescenceImages[i];
                        item.Dispose();
                    }
                    FlourescenceImages.Clear();
                }
                FlourescenceImages = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        ~ImageSetDto()
        {
            this.Dispose(false);
        }

        #endregion

        public object Clone()
        {
            var clone = (ImageSetDto) MemberwiseClone();

            if (BrightfieldImage != null)
                clone.BrightfieldImage = (ImageDto) BrightfieldImage.Clone();
            if (FlourescenceImages != null)
                clone.FlourescenceImages = FlourescenceImages.Select(f => (ImageDto) f.Clone()).ToList();

            return clone;
        }
    }
}