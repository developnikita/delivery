using CSharpFunctionalExtensions;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate
{
    public class StoragePlace : Entity<Guid>
    {
        public static StoragePlace Bag => new("Сумка", 10);

        [ExcludeFromCodeCoverage]
        private StoragePlace() { }

        private StoragePlace(string name, int volume)
        {
            Id = Guid.NewGuid();
            Name = name;
            TotalVolume = volume;
        }

        public string Name { get; }

        public int TotalVolume { get; }

        public Guid? OrderId { get; private set; }

        public static Result<StoragePlace, Error> Create(string name, int volume)
        {
            if (string.IsNullOrEmpty(name))
                return GeneralErrors.ValueIsInvalid(nameof(name));
            if (volume <= 0)
                return GeneralErrors.ValueIsInvalid(nameof(volume));

            return new StoragePlace(name, volume);
        }

        public Result<bool, Error> CanStorage(int volume)
        {
            if (volume <= 0)
                return GeneralErrors.ValueIsInvalid(nameof(volume));

            return !IsOccupied() && volume <= TotalVolume;
        }

        public UnitResult<Error> Store(Guid orderId, int volume)
        {
            var canStorage = CanStorage(volume);
            if (canStorage.IsFailure)
                return canStorage.Error;
            if (!canStorage.Value)
                return Errors.CannotPlaceOrderInStoragePlace();

            OrderId = orderId;
            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> Clear(Guid orderId)
        {
            if (!IsOccupied())
                return Errors.StoragePlaceIsEmpty();

            if (orderId != OrderId.Value)
                return Errors.StoragePlaceHoldOtherOrder(OrderId.Value, orderId);

            OrderId = null;
            return UnitResult.Success<Error>();
        }

        private bool IsOccupied()
        {
            return OrderId.HasValue;
        }

        public static class Errors
        {
            public static Error CannotPlaceOrderInStoragePlace()
            {
                return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.is.occupied.or.not.enought.space",
                    $"Нельзя разместить заказ в место хранения");
            }

            public static Error StoragePlaceIsEmpty()
            {
                return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.is.empty",
                    $"В месте хранения нет заказа");
            }

            public static Error StoragePlaceHoldOtherOrder(Guid holdOrder, Guid extractOrder)
            {
                return new Error($"{nameof(StoragePlace).ToLowerInvariant()}.hold.other.order",
                    $"В месте хранения находится другой заказ: {holdOrder}, не тот который будет извлечён: {extractOrder}");
            }
        }
    }
}
