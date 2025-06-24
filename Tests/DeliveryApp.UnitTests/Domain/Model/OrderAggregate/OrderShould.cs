using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate
{
    public class OrderShould
    {
        public static IEnumerable<object[]> GetErrorOrderData()
        {
            var orderId = Guid.NewGuid();
            var location = Location.Create(5, 6).Value;
            var volume = 5;
            yield return [Guid.Empty, location, volume];
            yield return [orderId, null, volume];
            yield return [orderId, location, 0];
            yield return [orderId, location, -1];
        }

        [Fact]
        public void BeCorrectWhenParamsAreCorrentOnCreated()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var location = Location.Create(5, 5).Value;
            var volume = 5;

            // Act
            var order = Order.Create(orderId, location, volume);

            // Assert
            order.IsSuccess.Should().BeTrue();
            order.Value.Id.Should().Be(orderId);
            order.Value.Location.Should().Be(location);
            order.Value.Volume.Should().Be(volume);
            order.Value.Status.Should().Be(OrderStatus.Created);
        }

        [Theory]
        [MemberData(nameof(GetErrorOrderData))]
        public void ReturnErrorWhenParamsAreNotCorrectOnCreated(Guid orderId, Location location, int volume)
        {
            // Arrange

            // Act
            var order = Order.Create(orderId, location, volume);

            // Assert
            order.IsFailure.Should().BeTrue();
            order.Error.Should().NotBeNull();
        }

        [Fact]
        public void AssignOrderToCourier()
        {
            // Arrange
            var courierLocation = Location.Create(5, 5).Value;
            var courier = Courier.Create("Иван", 4, courierLocation).Value;
            var orderLocation = Location.Create(1, 1).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 5).Value;

            // Act
            var result = order.Assign(courier);

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.CourierId.Should().Be(courier.Id);
            order.Status.Should().Be(OrderStatus.Assigned);
        }

        [Fact]
        public void ReturnErrorWhenAssignOrderToNullCourier()
        {
            // Arrange
            var orderLocation = Location.Create(1, 1).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 5).Value;

            // Act
            var result = order.Assign(null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenAssignAlreadyAssignedOrder()
        {
            // Arrange
            var courierLocation = Location.Create(5, 5).Value;
            var courier = Courier.Create("Иван", 4, courierLocation).Value;
            var orderLocation = Location.Create(1, 1).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 5).Value;

            // Act
            order.Assign(courier);
            var result = order.Assign(courier);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void CompleteOrder()
        {
            // Arrange
            var courierLocation = Location.Create(5, 5).Value;
            var courier = Courier.Create("Иван", 4, courierLocation).Value;
            var orderLocation = Location.Create(1, 1).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 5).Value;

            // Act
            order.Assign(courier);
            var result = order.Complete();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Completed);
        }

        [Fact]
        public void ReturnErrorWhenCompleteNotAssignedOrder()
        {
            // Arrange
            var orderLocation = Location.Create(1, 1).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 5).Value;

            // Act
            var result = order.Complete();

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }
    }
}
