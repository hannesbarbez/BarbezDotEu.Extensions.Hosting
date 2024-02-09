using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BarbezDotEu.Extensions.Hosting
{
    /// <summary>
    /// Base implementation of an <see cref="IHostedService"/> designed to support regular scoped execution of work using a Timer inside of
    /// its <see cref="DoWorkAsync(object)"/>. It may benefit from better garbage collection in some applications compared to
    /// e.g. <see cref="BackgroundService"/>, which is designed for long running tasks and may enjoy less favorable garbage
    /// collection than scoped work, in some cases.
    /// </summary>
    public abstract class ScopedHostedService : BaseScopedHostedService
    {
        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;
        private bool _available;
        private Timer _timer;

        /// <summary>
        /// Constructs a <see cref="ScopedHostedService"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>, used to e.g. create a scope from during <see cref="DoWorkAsync"/>.</param>
        /// <param name="dueTime">
        /// The amount of time to delay before <see cref="DoWorkAsync"/> is invoked, in milliseconds.
        /// Specify <see cref="Timeout.Infinite"/> to prevent the timer from starting.
        /// Specify zero (0) to start the timer immediately.
        /// </param>
        /// <param name="period">
        /// The time interval between invocations of <see cref="DoWorkAsync"/>, in milliseconds.
        /// Specify <see cref="Timeout.Infinite"/> to disable periodic signaling.
        /// </param>
        public ScopedHostedService(IServiceProvider serviceProvider, TimeSpan dueTime, TimeSpan period)
            : base(serviceProvider)
        {
            _dueTime = dueTime;
            _period = period;
            _available = true;
        }

        /// <inheritdoc/>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoWork, cancellationToken, _dueTime, _period);
            return base.StartAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Does the actual work. Use scope.ServiceProvider to get any 
        /// registered scoped, transient, or singleton services.
        /// </summary>
        /// <param name="scope">An async service scope to perform the work inside of.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that indicates that this <see cref="ScopedHostedService"/> has been aborted.</param>
        /// <returns>A <see cref="Task"/> representing the operation.</returns>
        protected abstract Task DoScopedWorkAsync(AsyncServiceScope scope, CancellationToken cancellationToken);

        /// <summary>
        /// Changes the start time and the interval between method invocations for a timer,
        /// using System.TimeSpan values to measure time intervals.
        /// </summary>
        /// <param name="dueTime">
        /// A System.TimeSpan representing the amount of time to delay before invoking the
        /// callback method specified when the System.Threading.Timer was constructed. Specify 
        /// negative one (-1) milliseconds to prevent the timer from restarting. Specify 
        /// zero (0) to restart the timer immediately.
        /// </param>
        /// <param name="period">The time interval between invocations of the callback method specified when the
        /// System.Threading.Timer was constructed. Specify negative one (-1) milliseconds
        /// to disable periodic signaling.</param>
        /// <returns>
        /// True if the timer was successfully updated; otherwise, false.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        ///  System.Threading.Timer has already been disposed.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The dueTime or period parameter, in milliseconds, is less than -1.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The dueTime or period parameter, in milliseconds, is greater than 4294967294.
        /// </exception>
        protected bool ChangeTimer(TimeSpan dueTime, TimeSpan period)
        {
            return this._timer.Change(dueTime, period);
        }

        private async Task DoWorkAsync(object state)
        {
            if (_available)
            {
                _available = false;
                var stoppingToken = (CancellationToken)state;
                using (var scope = ServiceProvider.CreateAsyncScope())
                {
                    await this.DoScopedWorkAsync(scope, stoppingToken);
                }

                _available = true;
            }
        }

        private void DoWork(object state)
        {
            DoWorkAsync(state).Wait();
        }
    }
}
