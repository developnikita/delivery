using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate
{
    public sealed class Courier : Aggregate<Guid>
    {
        [ExcludeFromCodeCoverage]
        private Courier() { }

        private Courier(string name, int speed, Location location, StoragePlace storagePlace)
        {
            Id = Guid.NewGuid();
            Name = name;
            Speed = speed;
            Location = location;
            StoragePlaces = [storagePlace];
        }

        public string Name { get; }

        public int Speed { get; }

        public Location Location { get; private set; }

        public List<StoragePlace> StoragePlaces { get; }

        public static Result<Courier, Error> Create(string name, int speed, Location location)
        {
            if (string.IsNullOrEmpty(name))
                return GeneralErrors.ValueIsRequired(nameof(name));

            if (speed <= 0)
                return GeneralErrors.ValueIsInvalid(nameof(speed));

            if (location == null)
                return GeneralErrors.ValueIsRequired(nameof(location));

            return new Courier(name, speed, location, StoragePlace.Bag);
        }

        public UnitResult<Error> AddStoragePlace(string name, int volume)
        {
            var newStorage = StoragePlace.Create(name, volume);
            if (newStorage.IsFailure)
                return newStorage.Error;

            StoragePlaces.Add(newStorage.Value);

            return UnitResult.Success<Error>();
        }

        public Result<bool, Error> CanTakeOrder(Order order)
        {
            if (order == null)
                return GeneralErrors.ValueIsRequired(nameof(order));

            return StoragePlaces.Exists(sp => sp.CanStorage(order.Volume).Value);
        }

        public UnitResult<Error> TakeOrder(Order order)
        {
            var canTakeOrder = CanTakeOrder(order);

            if (canTakeOrder.IsFailure)
                return canTakeOrder.Error;

            if (!canTakeOrder.Value)
                return UnitResult.Failure(Errors.AllStoragePlaceIsFull());

            var storagePlace = StoragePlaces.Single(sp => sp.CanStorage(order.Volume).Value);
            storagePlace.Store(order.Id, order.Volume);

            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> CompleteOrder(Order order)
        {
            if (order == null)
                return GeneralErrors.ValueIsRequired(nameof(order));

            var storageSpace = StoragePlaces.SingleOrDefault(sp => sp.OrderId == order.Id);
            if (storageSpace == null)
                return UnitResult.Failure(Errors.StoragePlaceDoesNotStoreThisOrder(order.Id));

            storageSpace.Clear(order.Id);

            return UnitResult.Success<Error>();
        }

        public Result<double, Error> CalculateTimeToLocation(Location location)
        {
            if (location == null)
                return GeneralErrors.ValueIsRequired(nameof(location));

            var distance = Location.DistanceTo(location);
            if (distance.IsFailure)
                return distance.Error;

            return distance.Value > 0
                   ? distance.Value > Speed
                     ? (double)distance.Value / Speed
                     : 1
                   : 0;
        }

        public UnitResult<Error> Move(Location target)
        {
            if (target == null)
                return GeneralErrors.ValueIsRequired(nameof(target));

            var difX = target.X - Location.X;
            var difY = target.Y - Location.Y;
            var cruisingRange = Speed;

            var moveX = Math.Clamp(difX, -cruisingRange, cruisingRange);
            cruisingRange -= Math.Abs(moveX);

            var moveY = Math.Clamp(difY, -cruisingRange, cruisingRange);

            var locationCreateResult = Location.Create(Location.X + moveX, Location.Y + moveY);
            if (locationCreateResult.IsFailure)
                return locationCreateResult.Error;

            Location = locationCreateResult.Value;

            return UnitResult.Success<Error>();
        }

        [ExcludeFromCodeCoverage]
        public static class Errors
        {
            public static Error AllStoragePlaceIsFull()
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.all.storage.place.is.full",
                    $"Все места хранения заняты");
            }

            public static Error StoragePlaceDoesNotStoreThisOrder(Guid orderId)
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.storage.place.does.not.store.this.order",
                    $"В местах хранения нет заказа: {orderId}");
            }
        }
    }
}
