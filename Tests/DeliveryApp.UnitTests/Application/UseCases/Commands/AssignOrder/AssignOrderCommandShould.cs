using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DeliveryApp.UnitTests.Application.UseCases.Commands.AssignOrder
{
    public class AssignOrderCommandShould
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICourierRepository _courierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDispatchService _dispatchService;
        private readonly AssignOrderHandler _handler;

        public AssignOrderCommandShould()
        {
            _orderRepository = Substitute.For<IOrderRepository>();
            _courierRepository = Substitute.For<ICourierRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _dispatchService = Substitute.For<IDispatchService>();
            _handler = new AssignOrderHandler(_orderRepository, _courierRepository, _unitOfWork, _dispatchService);
        }

        [Fact]
        public async Task ReturnFalseWhenNoUnassignedOrders()
        {
            // Arrange
            var courier = Courier.Create("Иван", 5, Location.Create(3, 3).Value).Value;
            var couriers = new[] { courier };

            _orderRepository.GetFirstInCreatedStatusAsync().Returns(Maybe.None);
            _courierRepository.GetAllFree().Returns(couriers);

            var command = new AssignOrderCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _orderRepository.DidNotReceive().Update(Arg.Any<Order>());
            _courierRepository.DidNotReceive().Update(Arg.Any<Courier>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReturnFalseWhenNoAvailableCouriers()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 5).Value;
            var couriers = new List<Courier>();

            _orderRepository.GetFirstInCreatedStatusAsync().Returns(order);
            _courierRepository.GetAllFree().Returns(couriers);

            var command = new AssignOrderCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();

            order.Status.Should().Be(OrderStatus.Created);

            _orderRepository.DidNotReceive().Update(Arg.Any<Order>());
            _courierRepository.DidNotReceive().Update(Arg.Any<Courier>());
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReturnFalseWhenDispatcherServiceIsFalure()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 5).Value;
            var courier = Courier.Create("Иван", 5, Location.Create(3, 3).Value).Value;
            var couriers = new[] { courier };

            _orderRepository.GetFirstInCreatedStatusAsync().Returns(order);
            _courierRepository.GetAllFree().Returns(couriers);

            _dispatchService.Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
                .Returns(Result.Failure<Courier, Error>(GeneralErrors.ValueIsInvalid("test error")));

            var command = new AssignOrderCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();

            order.Status.Should().Be(OrderStatus.Created);
            order.CourierId.Should().BeNull();

            _orderRepository.DidNotReceive().Update(order);
            _courierRepository.DidNotReceive().Update(courier);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ReturnTrueWhenOrderIsAssignToCourier()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 5).Value;
            var courier = Courier.Create("Иван", 5, Location.Create(3, 3).Value).Value;
            var couriers = new[] { courier };

            _orderRepository.GetFirstInCreatedStatusAsync().Returns(order);
            _courierRepository.GetAllFree().Returns(couriers);
            _dispatchService.Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
            .Returns(callInfo =>
            {
                courier.TakeOrder(order);
                order.Assign(courier);

                return Result.Success<Courier, Error>(courier);
            });
            _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

            var command = new AssignOrderCommand();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            order.Status.Should().Be(OrderStatus.Assigned);
            order.CourierId.Should().Be(courier.Id);

            courier.StoragePlaces.FirstOrDefault().OrderId.Should().Be(order.Id);

            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
