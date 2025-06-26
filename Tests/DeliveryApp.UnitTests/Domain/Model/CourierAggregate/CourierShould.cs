using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
    public class CourierShould
    {
        public static IEnumerable<object[]> GetErrorCourierData()
        {
            var name = "Иван";
            var speed = 4;
            var location = Location.Create(5, 5).Value;
            yield return ["", speed, location];
            yield return [name, 0, location];
            yield return [name, -1, location];
            yield return [name, speed, null];
        }

        public static Courier CreateCourier()
        {
            var name = "Иван";
            var speed = 4;
            var location = Location.Create(6, 6).Value;

            return Courier.Create(name, speed, location).Value;
        }

        public static Order CreateOrder()
        {
            var orderId = Guid.NewGuid();
            var orderLocation = Location.Create(1, 1).Value;
            var orderVolume = 3;

            return Order.Create(orderId, orderLocation, orderVolume).Value;
        }

        [Fact]
        public void BeCorrectWhenParamsAreCorrentOnCreated()
        {
            // Arrange
            var name = "Иван";
            var speed = 4;
            var location = Location.Create(6, 6).Value;

            // Act
            var courier = Courier.Create(name, speed, location);

            // Assert
            courier.IsSuccess.Should().BeTrue();
            courier.Value.Id.Should().NotBe(Guid.Empty);
            courier.Value.Name.Should().Be(name);
            courier.Value.Speed.Should().Be(speed);
            courier.Value.Location.Should().Be(location);
            courier.Value.StoragePlaces.Should().HaveCount(1);
        }

        [Theory]
        [MemberData(nameof(GetErrorCourierData))]
        public void ReturnErrorWhenParamsAreNotCorrectOnCreated(string name, int speed, Location location)
        {
            // Arrange

            // Act
            var courier = Courier.Create(name, speed, location);

            // Assert
            courier.IsFailure.Should().BeTrue();
            courier.Error.Should().NotBeNull();
        }

        [Fact]
        public void AddAnotherStorageBag()
        {
            // Arrange
            var bagName = "Ящик";
            var bagVolume = 4;
            var courier = CreateCourier();

            // Act
            var result = courier.AddStoragePlace(bagName, bagVolume);

            // Assert
            result.IsSuccess.Should().BeTrue();
            courier.StoragePlaces.Should().HaveCount(2);
            courier.StoragePlaces.Should().Contain(x => x.Name == bagName);
        }

        [Theory]
        [InlineData("", 5)]
        [InlineData("Ящик", 0)]
        [InlineData("Ящик", -1)]
        public void ReturnErrorWhenStoragePlactParamsAreNotCorrect(string bagName, int bagVolume)
        {
            // Arrange
            var courier = CreateCourier();

            // Act
            var result = courier.AddStoragePlace(bagName, bagVolume);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeTrueWhenCanTakeOrder()
        {
            // Arrange
            var courier = CreateCourier();
            var order = CreateOrder();

            // Act
            var result = courier.CanTakeOrder(order);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public void ReturnErrorWhenParamIsNullOnCanTakeOrder()
        {
            // Arrange
            var courier = CreateCourier();

            // Act
            var result = courier.CanTakeOrder(null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeFalseWhenOrderIsHeavier()
        {
            // Arrange
            var courier = CreateCourier();
            var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 11).Value;

            // Act
            var result = courier.CanTakeOrder(order);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public void BeCorrectWhenTakeOrder()
        {
            // Arrange
            var courier = CreateCourier();
            var order = CreateOrder();

            // Act
            var result = courier.TakeOrder(order);

            // Assert
            result.IsSuccess.Should().BeTrue();
            courier.StoragePlaces.Should().Contain(sp => sp.OrderId == order.Id);
        }

        [Fact]
        public void ReturnErrorWhenTakeNullOrder()
        {
            // Arrange
            var courier = CreateCourier();

            // Act
            var result = courier.TakeOrder(null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenTakeHeavierOrder()
        {
            // Arrange
            var courier = CreateCourier();
            var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 11).Value;

            // Act
            var result = courier.TakeOrder(order);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeCorrectWhenCompleteOrder()
        {
            // Arrange
            var courier = CreateCourier();
            var order = CreateOrder();

            // Act
            courier.TakeOrder(order);
            var result = courier.CompleteOrder(order);

            // Assert
            result.IsSuccess.Should().BeTrue();
            courier.StoragePlaces.Should().NotContain(sp => sp.OrderId == order.Id);
        }

        [Fact]
        public void ReturnErrorWhenCompleteNullOrder()
        {
            // Arrange
            var courier = CreateCourier();

            // Act
            var result = courier.CompleteOrder(null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenCompleteAnotherOrder()
        {
            // Arrange
            var o1 = CreateOrder();
            var o2 = CreateOrder();
            var courier = CreateCourier();

            // Act
            courier.TakeOrder(o1);
            var result = courier.CompleteOrder(o2);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeCorrectWhenCalculateTimeToLocation()
        {
            // Arrange
            var courier = CreateCourier();
            var location = Location.Create(3, 1).Value;

            // Act
            var result = courier.CalculateTimeToLocation(location);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(2);
        }

        [Fact]
        public void ReturnErrorWhenCalculateTimeToNullLocation()
        {
            // Arrange
            var courier = CreateCourier();

            // Act
            var result = courier.CalculateTimeToLocation(null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeChangeCourierLocationWhenMoveToTargetLocation()
        {
            // Arrange
            var courier = CreateCourier();
            var targetLocation = Location.Create(3, 1).Value;

            // Act
            var result = courier.Move(targetLocation);

            // Assert
            result.IsSuccess.Should().BeTrue();
            courier.Location.Should().Be(Location.Create(3, 5).Value);
        }
    }
}
