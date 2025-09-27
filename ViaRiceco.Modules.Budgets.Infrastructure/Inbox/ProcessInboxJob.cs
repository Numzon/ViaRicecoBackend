using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Domain.Inbox;
using ViaRiceco.Common.Infrastructure.Inbox;
using ViaRiceco.Common.Infrastructure.Serialization;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Inbox;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxJob(
    IUnitOfWork unitOfWork,
    IInboxMessageRepository inboxMessageRepository,
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
    IOptions<InboxOptions> inboxOptions,
    ILogger<ProcessInboxJob> logger) : IJob
{
    private const string ModuleName = "Budgets";
    private readonly InboxOptions _inboxOptions = inboxOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("{Module} - Beginning to process inbox messages", ModuleName);

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        IReadOnlyList<InboxMessage> inboxMessages =
            await inboxMessageRepository.GetInboxMessagesAsync(_inboxOptions.BatchSize, context.CancellationToken);

        if (inboxMessages.Count > 0)
        {
            foreach (InboxMessage inboxMessage in inboxMessages)
            {
                Exception? exception = null;

                try
                {
                    IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
                        inboxMessage.Content,
                        SerializerSettings.Instance)!;

                    using IServiceScope scope = serviceScopeFactory.CreateScope();

                    IEnumerable<IIntegrationEventHandler> handlers = IntegrationEventHandlersFactory.GetHandlers(
                        integrationEvent.GetType(),
                        scope.ServiceProvider,
                        Presentation.AssemblyReference.Assembly);

                    foreach (IIntegrationEventHandler handler in handlers)
                    {
                        await handler.Handle(integrationEvent, context.CancellationToken);
                    }
                }
                catch (Exception caughtException)
                {
                    logger.LogError(caughtException,
                        "{Module} - Exception while processing inbox message {InboxMessageId}",
                        ModuleName,
                        inboxMessage.Id);

                    exception = caughtException;
                }

                inboxMessage.Update(timeProvider.UtcNow(), exception);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("{Module} - Completed processing {Count} inbox messages", ModuleName, inboxMessages.Count);
        }

        await transaction.CommitAsync(context.CancellationToken);
        logger.LogInformation("{Module} - Completed processing inbox messages", ModuleName);
    }
}
