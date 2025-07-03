using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.EntityConfigurations.CourierAggregate
{
    internal class CourierEntityTypeConfiguration : IEntityTypeConfiguration<Courier>
    {
        public void Configure(EntityTypeBuilder<Courier> builder)
        {
            builder.ToTable("couriers");

            builder.HasKey(entity => entity.Id);

            builder.Property(entity => entity.Id)
                   .ValueGeneratedNever()
                   .HasColumnName("id")
                   .IsRequired();

            builder.Property(entity => entity.Name)
                   .HasColumnName("name")
                   .IsRequired();

            builder.Property(entity => entity.Speed)
                   .HasColumnName("speed")
                   .IsRequired();

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
