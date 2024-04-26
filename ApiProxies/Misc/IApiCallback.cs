namespace ApiProxies.Misc
{
    /// <summary>
    /// Inheritance root for all <see cref="ScoutInterface.ApiProxies.IApiResponse"/> interfaces.
    /// </summary>
    public interface IApiCallback
    {
    }

    /// <summary>
    /// This interface provides the basis for responses in a Command/Response Pattern for API function calls and callbacks.
    /// All responsibilities related to passive interactions with the HawkeyeCore API are encapsulated by
    /// implementations of this interface.
    /// </summary>
    public interface IApiCallback<TDelegate> : IApiCallback where TDelegate : class
    {
        TDelegate Callback { get; }
    }
}