using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Adapters.Postgres
{
    public class OrderRepositoryShould : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.5")
            .WithDatabase("order")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        private OrderRepository _orderRepository;
        private UnitOfWork _unitOfWork;
        private IMediator _mediator = Substitute.For<IMediator>();

        public OrderRepositoryShould() { }

        public async Task DisposeAsync()
        {
            await _postgreSqlContainer.DisposeAsync().AsTask();
        }

        public async Task InitializeAsync()
        {
            //Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
            await _postgreSqlContainer.StartAsync();

            //Накатываем миграции и справочники
            var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                    _postgreSqlContainer.GetConnectionString(),
                    sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
                .Options;

            var dbContext = new ApplicationDbContext(contextOptions);
            dbContext.Database.Migrate();

            _orderRepository = new OrderRepository(dbContext);
            _unitOfWork = new UnitOfWork(dbContext, _mediator);
        }

        [Fact]
        public async Task CanAddOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var location = Location.Create(1, 1).Value;
            var order = Order.Create(orderId, location, 5).Value;

            // Act
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var orderFromDb = await _orderRepository.GetAsync(orderId);
            orderFromDb.HasValue.Should().BeTrue();
            orderFromDb.Value.Should().BeEquivalentTo(order);
        }

        [Fact]
        public async Task CanUpdateOrder()
        {
            // Arrange
            var courierLocation = Location.Create(1, 1).Value;
            var courier = Courier.Create("Иван", 2, courierLocation).Value;

            var orderId = Guid.NewGuid();
            var orderLocation = Location.Create(5, 5).Value;
            var order = Order.Create(orderId, orderLocation, 3).Value;

            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var orderAssignResult = order.Assign(courier);
            orderAssignResult.IsSuccess.Should().BeTrue();

            _orderRepository.Update(order);

            // Assert
            var orderFromDb = await _orderRepository.GetAsync(order.Id);
            orderFromDb.HasValue.Should().BeTrue();
            orderFromDb.Value.Should().BeEquivalentTo(order);
        }

        [Fact]
        public async Task CanGetById()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderLocation = Location.Create(5, 5).Value;
            var order = Order.Create(orderId, orderLocation, 5).Value;

            // Act
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var getOrderResult = await _orderRepository.GetAsync(order.Id);
            getOrderResult.HasValue.Should().BeTrue();
            getOrderResult.Value.Should().BeEquivalentTo(order);

        }

        [Fact]
        public async Task CanGetListOrderInStatusAssigned()
        {
            // Arrange
            var courierLocation = Location.Create(1, 1).Value;
            var courier = Courier.Create("Иван", 2, courierLocation).Value;
            var orderList = new List<Order>()
            {
                Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 3).Value,
                Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 5).Value,
                Order.Create(Guid.NewGuid(), Location.Create(10, 10).Value, 1).Value,
            };

            orderList[2].Assign(courier);
            foreach (var o in orderList)
            {
                await _orderRepository.AddAsync(o);
            }
            await _unitOfWork.SaveChangesAsync();

            // Act
            var orders = await _orderRepository.GetAllInAssignedStatus();

            // Assert
            orders.Count().Should().Be(1);
            orders.Should().ContainEquivalentOf(orderList[2]);
        }

        [Fact]
        public async Task CanGetFirstInCreatedStatus()
        {
            // Arrange
            var courierLocation = Location.Create(1, 1).Value;
            var courier = Courier.Create("Иван", 2, courierLocation).Value;
            var orderList = new List<Order>()
            {
                Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 3).Value,
                Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value, 5).Value,
                Order.Create(Guid.NewGuid(), Location.Create(10, 10).Value, 1).Value,
            };

            orderList[2].Assign(courier);
            foreach (var o in orderList)
            {
                await _orderRepository.AddAsync(o);
            }
            await _unitOfWork.SaveChangesAsync();

            // Act
            var orders = await _orderRepository.GetFirstInCreatedStatusAsync();

            // Assert
            orders.Value.Should().BeOneOf(orderList[0], orderList[1]);
        }
    }
}
