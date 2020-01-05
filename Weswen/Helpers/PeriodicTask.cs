using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weswen
{
    /// <summary>
    /// Used for running an action periodically.
    /// </summary>
    public class PeriodicTask
    {
        /// <summary>
        /// Runs an action periodically with the support of cancelling.
        /// </summary>
        /// <param name="action">Action to run.</param>
        /// <param name="period">Delay between the invocations of the action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns></returns>
        public static async Task Run(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            // Run until the cancellation is requested
            while (!cancellationToken.IsCancellationRequested)
            {
                // Perform the action
                action();

                // Wait for the specified amount
                await Task.Delay(period, cancellationToken);
            }
        }

        /// <summary>
        /// Runs an action periodically.
        /// </summary>
        /// <param name="action">Action to run.</param>
        /// <param name="period">Delay between the invocations of the action.</param>
        /// <returns></returns>
        public static Task Run(Action action, TimeSpan period)
            => Run(action, period, CancellationToken.None);
    }
}
