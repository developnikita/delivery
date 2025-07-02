using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services
{
    public class DispatchServiceShould
    {
        private readonly DispatchService _service = new DispatchService();

        public static Courier CreateCourier(Location courierLocation)
        {
            var name = "Иван";
            var speed = 4;

            return Courier.Create(name, speed, courierLocation).Value;
        }

        public static Order CreateOrder(Location orderLocation)
        {
            return Order.Create(Guid.NewGuid(), orderLocation, 3).Value;
        }

        [Fact]
        public void ReturnErrorWhenOrderIsNull()
        {
            // Arrange
            var courierLocation = Location.Create(6, 6).Value;
            var couriers = new List<Courier> { CreateCourier(courierLocation) };

            // Act
            var result = _service.Dispatch(null, couriers);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenOrderIsAlreadyAssign()
        {
            // Arrange
            var courierLocation = Location.Create(6, 6).Value;
            var courier = CreateCourier(courierLocation);
            var couriers = new List<Courier> { courier };
            var orderLocation = Location.Create(3, 6).Value;
            var order = CreateOrder(orderLocation);

            // Act
            order.Assign(courier);
            var result = _service.Dispatch(order, couriers);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenCouriersIsNull()
        {
            // Arrange
            var orderLocation = Location.Create(3, 6).Value;
            var order = CreateOrder(orderLocation);

            // Act
            var result = _service.Dispatch(order, null);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorWhenCourierListIsEmpty()
        {
            // Arrange
            var orderLocation = Location.Create(3, 6).Value;
            var order = CreateOrder(orderLocation);

            // Act
            var result = _service.Dispatch(order, []);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnNeariestCourierToOrder()
        {
            // Arrange
            var cl1 = Location.Create(2, 2).Value;
            var c1 = CreateCourier(cl1);
            var cl2 = Location.Create(4, 4).Value;
            var c2 = CreateCourier(cl2);
            var couriers = new List<Courier> { c1, c2 };
            var orderLocation = Location.Create(9, 9).Value;
            var order = CreateOrder(orderLocation);

            // Act
            var result = _service.Dispatch(order, couriers);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(c2);

            order.Status.Should().Be(OrderStatus.Assigned);
            order.CourierId.Should().Be(c2.Id);

            c2.StoragePlaces.First().OrderId.Should().Be(order.Id);
        }
    }
}
