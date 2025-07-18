using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.Outbox
{
    public class OutboxMessageTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_message");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .ValueGeneratedNever()
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(entity => entity.Type)
                   .HasColumnName("type")
                   .IsRequired(false);

            builder.Property(entity => entity.Payload)
                   .HasColumnName("payload")
                   .IsRequired(false);

            builder.Property(entity => entity.OccurredAtUtc)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("occurred_at_utc")
                   .IsRequired();

            builder.Property(entity => entity.ProcessedAtUtc)
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .HasColumnName("processed_at_utc")
                   .IsRequired(false);
        }
    }
}
