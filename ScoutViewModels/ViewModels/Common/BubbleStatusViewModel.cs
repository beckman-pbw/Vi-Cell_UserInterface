using ScoutDomains;
using ScoutDomains.Common;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutViewModels.ViewModels.Common
{
    public class BubbleStatusViewModel : BaseViewModel
    {
        private string _imageProcessMessage;

        public string ImageProcessMessage
        {
            get { return _imageProcessMessage; }
            set
            {
                _imageProcessMessage = value;
                NotifyPropertyChanged(nameof(ImageProcessMessage));
            }
        }

        private IList<ImageProcessStatus> _imageProcessList;

        public IList<ImageProcessStatus> ImageProcessList
        {
            get { return _imageProcessList; }
            set
            {
                _imageProcessList = value;
                NotifyPropertyChanged(nameof(ImageProcessList));
            }
        }
        public void UpdateBubbleStatus(SampleRecordDomain selectedSampleRecord, bool isSampleRunning)
        {
            if(selectedSampleRecord?.SelectedSampleImageRecord == null)
                return;
            var imageProcessList = new List<ImageProcessStatus>();
            int cumulativeImageCount;
            int totalImageCount;
            if (isSampleRunning)
            {
                cumulativeImageCount = (int)selectedSampleRecord.NumImageSets;
                totalImageCount = selectedSampleRecord.SelectedSampleImageRecord.TotalCumulativeImage;
            }
            else
            {
                cumulativeImageCount = selectedSampleRecord.SelectedSampleImageRecord.TotalCumulativeImage;
                totalImageCount = (int)selectedSampleRecord.NumImageSets;
            }

            ImageProcessMessage = string.Format(LanguageResourceHelper.Get("LID_BubbleLabel_DiscardedMsg"),
                ScoutUtilities.Misc.ConvertToString(cumulativeImageCount), ScoutUtilities.Misc.ConvertToString(totalImageCount));
            foreach (var imageRecord in selectedSampleRecord.SampleImageList)
            {
                if (imageRecord.ResultPerImage != null)
                    switch (imageRecord.ResultPerImage.ProcessedStatus)
                    {
                        case E_ERRORCODE.eSuccess:
                            break;
                        case E_ERRORCODE.eBubbleImage:
                            UpdateImageProcessList(E_ERRORCODE.eBubbleImage, LanguageResourceHelper.Get("LID_eBubbleImage"), imageRecord, imageProcessList);
                            break;
                        case E_ERRORCODE.eInvalidBackgroundIntensity:
                            UpdateImageProcessList(E_ERRORCODE.eInvalidBackgroundIntensity, LanguageResourceHelper.Get("LID_eInvalidBackgroundIntensity"), imageRecord, imageProcessList);
                            break;
                        default:
                            UpdateImageProcessList(E_ERRORCODE.eDefault, LanguageResourceHelper.Get("LID_ImageProcessingError"), imageRecord, imageProcessList);
                            break;
                    }
            }

            ImageProcessList = imageProcessList;
        }

        private void UpdateImageProcessList(E_ERRORCODE imageErrorcode, string invalidMessage, SampleImageRecordDomain imageRecord, IList<ImageProcessStatus> imageProcessList)
        {
            var imageItem = imageProcessList.FirstOrDefault(x => x.ImageErrorCode == imageErrorcode);
            if (imageItem == null)
            {
                imageProcessList.Add(new ImageProcessStatus(imageErrorcode, invalidMessage, ScoutUtilities.Misc.ConvertToString(imageRecord.SequenceNumber)));
            }
            else
            {
                imageItem.ImageErrorCount = imageItem.ImageErrorCount + ", " + ScoutUtilities.Misc.ConvertToString(imageRecord.SequenceNumber);
            }
        }



    }
}
