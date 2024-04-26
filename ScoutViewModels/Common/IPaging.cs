using ScoutUtilities;

namespace ScoutViewModels.Common
{
    public interface IPaging
    {
        RelayCommand PageFullBackCommand { get; }
        RelayCommand PageBackCommand { get; }
        RelayCommand PageForwardCommand { get; }
        RelayCommand PageFullForwardCommand { get; }
    }
}