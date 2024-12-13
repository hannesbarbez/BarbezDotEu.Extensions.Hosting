# BarbezDotEu.Extensions.Hosting
Misc. extensions to Microsoft.Extensions.Hosting, such as ScopedHostedService.

## ScopedHostedService
ScopedHostedService is an IHostedService implementation designed to support regular scoped execution of work using a Timer.

The main benefit is a better garbage collection in some applications compared to the e.g. the native .NET BackgroundService
(which seems designed for long running tasks and may enjoy less favorable garbage collection than scoped work, in some cases).

If your app requires many iterations of smaller jobs, try using this ScopedHostedService to avoid the excessive memory consumption seen in some other implementations of IHostedService.