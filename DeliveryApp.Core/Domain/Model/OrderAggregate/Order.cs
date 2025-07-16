using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate
{
    public sealed class Order : Aggregate<Guid>
    {
        [ExcludeFromCodeCoverage]
        private Order() { }

        private Order(Guid orderId, Location location, int volume)
        {
            Id = orderId;
            Location = location;
            Volume = volume;
            Status = OrderStatus.Created;
        }

        public Location Location { get; }

        public int Volume { get; }

        public OrderStatus Status { get; private set; }

        public Guid? CourierId { get; private set; }

        public static Result<Order, Error> Create(Guid orderId, Location location, int volume)
        {
            if (orderId == Guid.Empty)
                return GeneralErrors.ValueIsRequired(nameof(orderId));

            if (location == null)
                return GeneralErrors.ValueIsRequired(nameof(location));

            if (volume <= 0)
                return GeneralErrors.ValueIsRequired(nameof(volume));

            return new Order(orderId, location, volume);
        }

        public UnitResult<Error> Assign(Courier courier)
        {
            if (courier == null)
                return GeneralErrors.ValueIsRequired(nameof(courier));

            if (Status != OrderStatus.Created)
                return Errors.OrderIsAlreadyAssigned();

            CourierId = courier.Id;
            Status = OrderStatus.Assigned;

            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> Complete()
        {
            if (Status != OrderStatus.Assigned)
                return Errors.OrderIsNotAssigned();

            if (CourierId == null)
                return Errors.OrderIsNotAssigned();

            Status = OrderStatus.Completed;

            RaiseDomainEvent(new OrderCompletedDomainEvent(Id, Status.Name));
            return UnitResult.Success<Error>();
        }

        [ExcludeFromCodeCoverage]
        public static class Errors
        {
            public static Error OrderIsAlreadyAssigned()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.is.already.assigned",
                    $"Заказ уже назначен на курьера");
            }

            public static Error OrderIsNotAssigned()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.is.not.assigned",
                    $"Заказ не назначен");
            }
        }
    }
}
