namespace ScoutModels.Interfaces
{
    public interface IDisplayService
    {
        void DisplayMessage(string message, bool showPrompt = true);
    }
}