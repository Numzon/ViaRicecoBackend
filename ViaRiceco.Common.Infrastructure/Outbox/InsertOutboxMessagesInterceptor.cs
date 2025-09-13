using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using ViaRiceco.Common.Domain.Interfaces;
using ViaRiceco.Common.Domain.Models;
using ViaRiceco.Common.Domain.Outbox;
using ViaRiceco.Common.Infrastructure.Serialization;

namespace ViaRiceco.Common.Infrastructure.Outbox;

public class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            InsertOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext context)
    {
        var outboxMessages = context
            .ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IDomainEvent> domainEvents = entity.DomainEvents.ToList();

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .Select(de =>
                OutboxMessage.Create(de.Id, de.GetType().Name,
                    JsonConvert.SerializeObject(de, SerializerSettings.Instance), de.OccurredOnUtc)
            ).ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
