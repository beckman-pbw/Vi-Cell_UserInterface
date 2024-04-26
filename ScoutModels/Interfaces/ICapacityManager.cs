namespace ScoutModels.Interfaces
{
    public interface ICapacityManager
    {
        bool InsufficientReagentPackUsesLeft(ushort queueLength, bool showPrompt = true);
        bool InsufficientDisposalTrayCapacity(ushort queueLength, bool showPrompt = true);
    }
}