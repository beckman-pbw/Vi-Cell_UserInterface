namespace ScoutModels.Service
{
    /// <summary>
    /// The SampleSetService registers for Sample and SampleSet events in the RunningWorkListModel.
    /// It captures and retains all new SampleSet(s), in Pending or Running state, as these currently cannot be
    /// retrieved from the backend. This class also contains methods to retrieve all completed
    /// sample sets, either unfiltered, or filtered by user. These two lists of samples can be
    /// retrieved by the HomeViewModel to be displayed.
    /// </summary>
    public class SampleSetService
    {
        
    }
}