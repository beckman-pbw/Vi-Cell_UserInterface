using System;
using System.Threading.Tasks;
using Ninject.Extensions.Logging;
using ScoutModels.Interfaces;

namespace ScoutModels.Security
{
    /// <summary>
    /// @todo - remove this - not using security principles 
    /// 
    /// </summary>
    public class SecuredTask
    {
        private readonly ILogger _logger;

        public SecuredTask(ILogger logger)
        {
            _logger = logger;
        }

        public Task Run(Action action)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    action.Invoke();
                }
                finally
                {
                }
            });

            return task;
        }

        //
        // Summary:
        //     Queues the specified work to run on the thread pool and returns a proxy for the
        //     Task(TResult) returned by function.
        //
        // Parameters:
        //   function:
        //     The work to execute asynchronously
        //
        // Type parameters:
        //   TResult:
        //     The type of the result returned by the proxy task.
        //
        // Returns:
        //     A Task(TResult) that represents a proxy for the Task(TResult) returned by function.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The function parameter was null.
        public Task<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = Task.Run<TResult>(() =>
            {
                try
                {
                    return function.Invoke();
                }
                finally
                {
                }
            });

            return task;

        }
    }
}