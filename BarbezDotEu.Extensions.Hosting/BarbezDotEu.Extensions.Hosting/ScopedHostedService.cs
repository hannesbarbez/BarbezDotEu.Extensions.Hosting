using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BarbezDotEu.Extensions.Hosting
{
    /// <summary>
    /// Base implementation of an <see cref="IHostedService"/> designed to support scoped execution of work inside its <see cref="DoWorkAsync(object)"/>.
    /// It may benefit from better garbage collection in some applications compared to e.g. <see cref="BackgroundService"/>,
    /// which is designed for long running tasks and may enjoy less favorable garbage collection than scoped work, in some cases.
    /// </summary>
    public abstract class ScopedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;
        private Timer timer;

        /// <summary>
        /// Gets an instance of <see cref="IServiceProvider"/>, used to e.g. create a scope from during <see cref="DoWorkAsync"/>.
        /// </summary>
        protected IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Constructs a <see cref="ScopedHostedService"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used to e.g. create a scope from during <see cref="DoWorkAsync"/>.</param>
        /// <param name="dueTime">
        /// The amount of time to delay before callback is invoked, in milliseconds.
        /// Specify <see cref="Timeout.Infinite"/> to prevent the timer from starting.
        /// Specify zero (0) to start the timer immediately.
        /// </param>
        /// <param name="period">
        /// The time interval between invocations of callback, in milliseconds.
        /// Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.
        /// </param>
        public ScopedHostedService(IServiceProvider serviceProvider, TimeSpan dueTime, TimeSpan period)
        {
            _serviceProvider = serviceProvider;
            _dueTime = dueTime;
            _period = period;
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(DoWork, cancellationToken, _dueTime, _period);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does the actual work.
        /// </summary>
        /// <remarks>
        /// It is recommended to first create a scope using e.g. <see cref="IServiceProvider"/>. For this, you'd need to add a dependency on e.g. <see cref="Microsoft.Extensions.DependencyInjection"/>.
        /// </remarks>
        /// <param name="state">A <see cref="CancellationToken"/> that indicates that this <see cref="ScopedHostedService"/> has been aborted.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        protected abstract Task DoWorkAsync(object state);

        private void DoWork(object state)
        {
            DoWorkAsync(state).Wait();
        }
    }
}
