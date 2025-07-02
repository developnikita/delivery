using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services.DispatchService
{
    public class DispatchService : IDispatchService
    {
        public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers)
        {
            if (order == null)
                return GeneralErrors.ValueIsRequired(nameof(order));

            if (couriers == null)
                return GeneralErrors.ValueIsRequired(nameof(couriers));

            if (order.Status != OrderStatus.Created)
                return Order.Errors.OrderIsAlreadyAssigned();

            var availableCourier = couriers.Where(c => c.CanTakeOrder(order).Value)
                .OrderBy(c => c.CalculateTimeToLocation(order.Location).Value)
                .FirstOrDefault();

            if (availableCourier == null)
                return Errors.NotAvailableCourier();

            var takeOrderResult = availableCourier.TakeOrder(order);
            if (takeOrderResult.IsFailure)
                return takeOrderResult.Error;

            var orderAssignResult = order.Assign(availableCourier);
            if (orderAssignResult.IsFailure)
                return orderAssignResult.Error;

            return availableCourier;
        }

        public static class Errors
        {
            public static Error NotAvailableCourier()
            {
                return new Error($"{nameof(DispatchService).ToLowerInvariant()}.not.available.courier",
                    $"Все курьеры заняты");
            }
        }
    }
}
