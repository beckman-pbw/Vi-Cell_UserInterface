namespace ScoutModels.Common
{
    public enum ApplicationStateEnum
    {
        Startup,
        Shutdown
    }

    public class ApplicationStateChange
    {
        /// <summary>
        /// New state the application is transitioning to.
        /// </summary>
        public ApplicationStateEnum State { get; set; }

        /// <summary>
        /// Only present if State is Shutdown.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Only present if State is Shutdown.
        /// </summary>
        public bool Restart { get; set; }

        public ApplicationStateChange(ApplicationStateEnum state, string reason, bool restart)
        {
            State = state;
            Reason = reason;
            Restart = restart;
        }
    }

}