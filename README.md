# BarbezDotEu.Extensions.Hosting
Misc. extensions to Microsoft.Extensions.Hosting, such as ScopedHostedService.

## ScopedHostedService
ScopedHostedService is an IHostedService implementation designed to support scoped execution of work.

It may benefit from better garbage collection in some applications compared to e.g. BackgroundService which is designed for long running tasks and may enjoy less favorable garbage collection than scoped work, in some cases.
		