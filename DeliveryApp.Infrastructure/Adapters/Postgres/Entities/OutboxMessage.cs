namespace DeliveryApp.Infrastructure.Adapters.Postgres.Entities
{
    // Стоит добавить AssemblyQualifiedName
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public DateTime OccurredAtUtc { get; set; }

        public DateTime? ProcessedAtUtc { get; set; }

    }
}
