using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BarbezDotEu.Extensions.Hosting
{
    /// <inheritdoc/>
    public abstract class BaseScopedHostedService : IHostedService
    {
        /// <summary>
        /// Gets an instance of <see cref="IServiceProvider"/>, used to e.g. create a scope from.
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets a value indicating whether the process has been aborted.
        /// </summary>
        protected virtual CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Constructs a <see cref="BaseScopedHostedService"/>.
        /// </summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        protected BaseScopedHostedService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }
}