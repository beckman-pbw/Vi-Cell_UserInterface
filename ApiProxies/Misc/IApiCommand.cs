using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;

namespace ApiProxies.Misc
{
    /// <summary>
    /// This interface provides the basis for commands in a Command/Response pattern for API interactions.
    /// All responsibilities related to active interactions with the HawkeyeCore API are encapsulated by
    /// implementations of this interface.
    /// </summary>
    public interface IApiCommand : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this API method type is responsible for allocating and freeing
        /// memory.
        /// </summary>
        bool ManagesMemory { get; }

        /// <summary>
        /// Gets a value indicating whether this API method has been invoked yet.
        /// </summary>
        bool IsInvoked { get; }

        /// <summary>
        /// Gets/sets a value indicating whether this class automatically releases any unmanaged memory that it holds.
        /// Defaults to true.
        /// </summary>
        bool AutoReleaseMemory { get; set; }

        /// <summary>
        /// Gets a list of exceptions caused by a call to Invoke. The list may be empty.
        /// </summary>
        List<Exception> InvocationExceptions { get; }

        /// <summary>
        /// Gets a value that indicates whether this IApiCommand experienced an exception during invocation.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// Gets the result of this proxy being invoked.
        /// </summary>
        HawkeyeError Result { get; }

        /// <summary>
        /// Executes the API function for which this IApiCommand is responsible
        /// </summary>
        /// <returns>Result in the form of a HawkeyeError</returns>
        HawkeyeError Invoke();
    }
}