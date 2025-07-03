using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate
{
    internal class StoragePlaceEntityTypeConfiguration : IEntityTypeConfiguration<StoragePlace>
    {
        public void Configure(EntityTypeBuilder<StoragePlace> builder)
        {
            builder.ToTable("storage_places");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .ValueGeneratedNever()
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(entity => entity.Name)
                   .HasColumnName("name")
                   .IsRequired();

            builder.Property(entity => entity.TotalVolume)
                   .HasColumnName("total_volume")
                   .IsRequired();

            builder.Property(entity => entity.OrderId)
                   .HasColumnName("order_id")
                   .IsRequired(false);

            builder.Property("CourierId")
                   .HasColumnName("courier_id")
                   .IsRequired();
        }

    }
}
