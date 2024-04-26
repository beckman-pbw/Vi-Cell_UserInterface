using System;
using System.Collections.Generic;
using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    public enum ExpandedContentType
    {
        Image,
        BarChart,
        ScatterChart
    }

    public class ExpandedImageGraphEventArgs : BaseDialogEventArgs
    {
        public ExpandedContentType ContentType { get; set; }
        public ImageType SelectedImageType { get; set; }
        public string SelectedRightClickImageType { get; set; }

        public List<object> BarGraphDomainList { get; set; }
        public object SelectedBarGraphDomain { get; set; }

        public List<object> SampleImageRecordDomains { get; set; }
        public object SelectedSampleRecordDomain { get; set; }

        public object SampleImageRecordDomain { get; set; }

        public bool IsQueueManagementDialog { get; set; }
        public bool IsHorizontalPaginationVisible { get; set; }
        public bool IsSetFocusEnable { get; set; }
        public bool IsListAvailable { get; set; }
        public bool IsExpandedResultListVisible { get; set; }

        public int SelectedImageIndex { get; set; }
        public int SelectedGraphIndex { get; set; }

        public bool ShowSlideShowButtons { get; set; }

        public ExpandedImageGraphEventArgs(ExpandedContentType contentType, string selectedRightClickImageType, List<object> barGraphDomainList, object selectedBarGraphDomain, 
            int selectedGraphIndex = -1, bool showSlideShowButtons = true)
        {
            ContentType = contentType;
            SelectedRightClickImageType = selectedRightClickImageType;
            BarGraphDomainList = barGraphDomainList;
            SelectedBarGraphDomain = selectedBarGraphDomain;
            SelectedGraphIndex = selectedGraphIndex;
            ShowSlideShowButtons = showSlideShowButtons;
            
            SelectedImageType = ImageType.Annotated;
            SetDefaultBool();
        }

        public ExpandedImageGraphEventArgs(ImageType imageType, string selectedRightClickImageType, List<object> sampleImageRecordDomains, object selectedSampleRecordDomain,
            int selectedImageIndex = -1, bool showSlideShowButtons = true)
        {
            SelectedRightClickImageType = selectedRightClickImageType;
            SampleImageRecordDomains = sampleImageRecordDomains;
            SelectedSampleRecordDomain = selectedSampleRecordDomain;
            SelectedImageIndex = selectedImageIndex;
            SelectedImageType = imageType;
            ShowSlideShowButtons = showSlideShowButtons;

            ContentType = ExpandedContentType.Image;
            SetDefaultBool();
        }

        private void SetDefaultBool()
        {
            IsSetFocusEnable = false;
            IsQueueManagementDialog = false;
            IsExpandedResultListVisible = false;
            IsHorizontalPaginationVisible = true;
            IsListAvailable = true;
        }

        public ExpandedImageGraphEventArgs(ExpandedContentType contentType, ImageType imageType, string selectedRightClickImageType, List<object> barGraphDomainList,
            object selectedBarGraphDomain, object selectedSampleRecordDomain, object sampleImageRecordDomain = null, bool isQueueManagementDialog = false, 
            bool visibleHorizontalPagination = true, bool isSetFocusEnabled = false, bool isListAvailable = true, bool isExpandedResultListVisible = false, 
            bool showSlideShowButtons = true)
        {
            ContentType = contentType;
            SelectedImageType = imageType;
            SelectedRightClickImageType = selectedRightClickImageType;
            BarGraphDomainList = barGraphDomainList;
            SelectedBarGraphDomain = selectedBarGraphDomain;
            SelectedSampleRecordDomain = selectedSampleRecordDomain;
            SampleImageRecordDomain = sampleImageRecordDomain;
            IsQueueManagementDialog = isQueueManagementDialog;
            IsHorizontalPaginationVisible = visibleHorizontalPagination;
            IsSetFocusEnable = isSetFocusEnabled;
            IsListAvailable = isListAvailable;
            IsExpandedResultListVisible = isExpandedResultListVisible;
            ShowSlideShowButtons = showSlideShowButtons;
        }
    }
}