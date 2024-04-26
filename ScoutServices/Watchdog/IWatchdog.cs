namespace ScoutServices.Watchdog
{
    public interface IWatchdog
    {
        void Start();
        void Stop(bool clearAllWatches = false);
        void AddWatch(string fullPathToExeWithExtension);
        void RemoveWatch(string processName);
        void ClearAllWatches();
        void AddAllWatches();
    }
}
