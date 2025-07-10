using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.OrderAggregate
{
    internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .ValueGeneratedNever()
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(entity => entity.CourierId)
                   .HasColumnName("courier_id")
                   .IsRequired(false);

            builder.Property(entity => entity.Volume)
                    .HasColumnName("volume")
                    .IsRequired(true);

            builder.OwnsOne(entity => entity.Status, a =>
                   {
                       a.Property(c => c.Name).HasColumnName("status").IsRequired();
                       a.WithOwner();
                   });
            builder.Navigation(entity => entity.Status).IsRequired();

            builder.OwnsOne(entity => entity.Location, l =>
                   {
                       l.Property(x => x.X).HasColumnName("location_x").IsRequired();
                       l.Property(y => y.Y).HasColumnName("location_y").IsRequired();
                       l.WithOwner();
                   });
            builder.Navigation(entity => entity.Location).IsRequired();
        }
    }
}
