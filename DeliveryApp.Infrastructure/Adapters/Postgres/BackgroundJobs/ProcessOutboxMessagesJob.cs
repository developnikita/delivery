using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Primitives;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class ProcessOutboxMessagesJob : IJob
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPublisher _publisher;

        public ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IPublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var outboxMessages = await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedAtUtc == null)
                .OrderBy(o => o.OccurredAtUtc)
                .Take(20)
                .ToListAsync(context.CancellationToken);

            if (outboxMessages.Any())
            {
                foreach (var outboxMessage in outboxMessages)
                {
                    var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Payload,
                        new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        });

                    await _publisher.Publish(domainEvent);
                    outboxMessage.ProcessedAtUtc = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
