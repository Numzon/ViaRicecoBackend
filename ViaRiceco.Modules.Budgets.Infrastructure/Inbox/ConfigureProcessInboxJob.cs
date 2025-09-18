using Microsoft.Extensions.Options;
using Quartz;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Inbox;

internal sealed class ConfigureProcessInboxJob(IOptions<InboxOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    private readonly InboxOptions _inboxOptions = options.Value;

    public void Configure(QuartzOptions options)
    {
        string jobName = typeof(ProcessInboxJob).FullName!;

        options
            .AddJob<ProcessInboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(_inboxOptions.IntervalInSeconds).RepeatForever()));
    }
}

