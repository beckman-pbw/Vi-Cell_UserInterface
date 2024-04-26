using System;

namespace ApiProxies.Misc
{
    /// <summary>
    /// </summary>
    public interface IApiEvent : IDisposable
    {
        ApiEventType EventType { get; }
    }
}