using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using ViaRiceco.Common.Application.EventBus;
using ViaRiceco.Common.Application.Extensions;
using ViaRiceco.Common.Application.Messaging;
using ViaRiceco.Common.Domain.Interfaces;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Outbox;
using ViaRiceco.Common.Infrastructure.Serialization;
using ViaRiceco.Modules.Budgets.Application.Abstractions.Data;
using ViaRiceco.Modules.Budgets.Domain.Outbox;

namespace ViaRiceco.Modules.Budgets.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IUnitOfWork unitOfWork,
    IOutboxMessageRepository outboxMessageRepository,
    IServiceScopeFactory serviceScopeFactory,
    TimeProvider timeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
    private const string ModuleName = "Budgets";
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("{Module} - Beginning to process outbox messages", ModuleName);

        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        IReadOnlyList<OutboxMessage> outboxMessages =
            await outboxMessageRepository.GetOutboxMessagesAsync(_outboxOptions.BatchSize, context.CancellationToken);

        if (outboxMessages.Count > 0)
        {
            foreach (OutboxMessage outboxMessage in outboxMessages)
            {
                Exception? exception = null;

                try
                {
                    IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                        outboxMessage.Content,
                        SerializerSettings.Instance)!;

                    using IServiceScope scope = serviceScopeFactory.CreateScope();

                    IEnumerable<IDomainEventHandler> domainEventHandlers = DomainEventHandlersFactory.GetHandlers(
                        domainEvent.GetType(),
                        scope.ServiceProvider,
                        Application.AssemblyReference.Assembly);

                    foreach (IDomainEventHandler domainEventHandler in domainEventHandlers)
                    {
                        await domainEventHandler.Handle(domainEvent, CancellationToken.None);
                    }
                }
                catch (Exception caughtException)
                {
                    logger.LogError(caughtException,
                        "{Module} - Exception while processing outbox message {OutboxMessageId}",
                        ModuleName,
                        outboxMessage.Id);

                    exception = caughtException;
                }

                outboxMessage.Update(timeProvider.UtcNow(), exception);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("{Module} - Completed processing {Count} outbox messages", ModuleName, outboxMessages.Count);
        }

        await transaction.CommitAsync(context.CancellationToken);
        logger.LogInformation("{Module} - Completed processing outbox messages", ModuleName);
    }
}
