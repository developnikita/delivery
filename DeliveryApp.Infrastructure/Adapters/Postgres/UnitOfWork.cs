using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using MediatR;
using Newtonsoft.Json;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private ApplicationDbContext _dbContext;

        private bool _disposed;

        public UnitOfWork(ApplicationDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await SaveDomainEventsInOutboxMessageAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task SaveDomainEventsInOutboxMessageAsync(CancellationToken cancellationToken)
        {
            var outboxMessages = _dbContext.ChangeTracker
                .Entries<IAggregateRoot>()
                .Select(e => e.Entity)
                .SelectMany(aggregate =>
                {
                    var domainEvents = aggregate.GetDomainEvents();

                    aggregate.ClearDomainEvents();
                    return domainEvents;
                })
                .Select(domainEvent => new OutboxMessage
                {
                    Id = domainEvent.EventId,
                    Type = domainEvent.GetType().Name,
                    Payload = JsonConvert.SerializeObject(
                        domainEvent,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        }),
                    OccurredAtUtc = DateTime.UtcNow,
                })
                .ToList();

            await _dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) _dbContext.Dispose();
                _disposed = true;
            }
        }
    }
}
