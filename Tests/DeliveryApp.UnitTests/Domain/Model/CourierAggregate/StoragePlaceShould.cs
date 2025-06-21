using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
    public class StoragePlaceShould
    {
        public static IEnumerable<object[]> GetErrorStoragePlaceData()
        {
            var storageName = "Сумка";
            var storageVolume = 10;
            yield return [null, storageVolume];
            yield return ["", storageVolume];
            yield return [storageName, 0];
            yield return [storageName, -1];
        }

        [Fact]
        public void BeCorrectWhenParamsAreCorrectOnCreated()
        {
            // Arrange
            var storageName = "Сумка";
            var storageVolume = 10;

            // Act
            var storagePlace = StoragePlace.Create(storageName, storageVolume);

            // Assert
            storagePlace.IsSuccess.Should().BeTrue();
            storagePlace.Value.Name.Should().Be(storageName);
            storagePlace.Value.TotalVolume.Should().Be(storageVolume);
            storagePlace.Value.OrderId.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(GetErrorStoragePlaceData))]
        public void ReturnErrorWhenParamsAreNotCorrectOnCreated(string name, int volume)
        {
            // Arrange

            // Act
            var storagePlace = StoragePlace.Create(name, volume);

            // Assert
            storagePlace.IsSuccess.Should().BeFalse();
            storagePlace.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeEqualWhenStoragePlaceIdIsEqual()
        {
            // Arrange
            var sp1 = StoragePlace.Create("Багажник", 10).Value;
            var sp2 = sp1;

            // Act
            var result = sp2.Equals(sp1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void BeNotEqualWhenStoragePlaceIdIsDifferent()
        {
            // Arrange
            var sp1 = StoragePlace.Create("Багажник", 10).Value;
            var sp2 = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var result = sp1.Equals(sp2);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        public void CanStorageInEmpryStoragePlace(int volume)
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var canStorage = storagePlace.CanStorage(volume);

            // Assert
            canStorage.IsSuccess.Should().BeTrue();
            canStorage.Value.Should().BeTrue();
        }

        [Fact]
        public void CannotStorageWhenOrderVolumeIsBiggerThanStorage()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var canStorage = storagePlace.CanStorage(11);

            // Assert
            canStorage.IsSuccess.Should().BeTrue();
            canStorage.Value.Should().BeFalse();
        }

        [Fact]
        public void CannotStorageWhenOrderIsAlreadyPlacedInStorage()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            storagePlace.Store(Guid.NewGuid(), 9);
            var canStorage = storagePlace.CanStorage(5);

            // Assert
            canStorage.IsSuccess.Should().BeTrue();
            canStorage.Value.Should().BeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void ReturnErrorOnCanStorageWhenParamsAreNotCorrect(int volume)
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var canStorage = storagePlace.CanStorage(volume);

            // Assert
            canStorage.IsSuccess.Should().BeFalse();
            canStorage.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeCorrectWhenStoreCorrectOrder()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;
            var orderId = Guid.NewGuid();

            // Act
            var result = storagePlace.Store(orderId, 5);

            // Assert
            result.IsSuccess.Should().BeTrue();
            storagePlace.OrderId.Should().Be(orderId);
        }

        [Fact]
        public void ReturnErrorOnStoreWhenStoragePlaceIsOccupied()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            storagePlace.Store(Guid.NewGuid(), 5);
            var result = storagePlace.Store(Guid.NewGuid(), 5);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorOnStoreWhenStoragePlaceIsNotEnoughtSpace()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var result = storagePlace.Store(Guid.NewGuid(), 12);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public void ReturnErrorOnStoreWhenVolumeNotCorrect(int volume)
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;
            var orderId = Guid.NewGuid();

            // Act
            var result = storagePlace.Store(Guid.NewGuid(), volume);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorOnClearWhenOrderIsNotInStoragePlace()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            var result = storagePlace.Clear(Guid.NewGuid());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenClearOtherOrder()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;

            // Act
            storagePlace.Store(Guid.NewGuid(), 5);
            var result = storagePlace.Clear(Guid.NewGuid());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeCorrectWhenClearStoragePlace()
        {
            // Arrange
            var storagePlace = StoragePlace.Create("Багажник", 10).Value;
            var orderId = Guid.NewGuid();

            // Act
            storagePlace.Store(orderId, 5);
            var result = storagePlace.Clear(orderId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            storagePlace.OrderId.Should().BeNull();
        }
    }
}
