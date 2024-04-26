using ScoutUtilities.Common;

namespace ScoutModels.ExpandedSampleWorkflow
{
    public class SampleModel
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string SampleSetName { get; set; }

        public bool IsCarousel { get; set; }
        public int CarouselPosition { get; set; }
        public SamplePosition SamplePosition { get; set; }

        public SampleModel(SamplePosition samplePosition, string name, string username, string sampleSetName)
        {
            SamplePosition = samplePosition;
            IsCarousel = samplePosition.IsCarousel();
            CarouselPosition = IsCarousel ? samplePosition.Column : 0;
            
            Name = name;
            Username = username;
            SampleSetName = sampleSetName;
        }
    }
}