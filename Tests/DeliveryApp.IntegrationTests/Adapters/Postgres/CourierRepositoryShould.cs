using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Adapters.Postgres
{
    public class CourierRepositoryShould : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:17.5")
            .WithDatabase("order")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        private CourierRepository _courierRepository;
        private UnitOfWork _unitOfWork;

        public CourierRepositoryShould() { }

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

            _courierRepository = new CourierRepository(dbContext);
            _unitOfWork = new UnitOfWork(dbContext);
        }

        [Fact]
        public async Task CanAddCourier()
        {
            // Arrange
            var courierLocation = Location.Create(2, 2).Value;
            var courier = Courier.Create("Иван", 2, courierLocation).Value;

            // Act
            await _courierRepository.AddAsync(courier);
            await _unitOfWork.SaveChangesAsync();

            // Assert
            var courierFromDb = await _courierRepository.GetAsync(courier.Id);
            courierFromDb.HasValue.Should().BeTrue();
            courierFromDb.Value.Should().BeEquivalentTo(courier);
        }

        [Fact]
        public async Task CanUpdateCourier()
        {
            // Arrange
            var courierLocation = Location.Create(2, 2).Value;
            var courier = Courier.Create("Иван", 2, courierLocation).Value;
            await _courierRepository.AddAsync(courier);
            await _unitOfWork.SaveChangesAsync();
            var targetLocation = Location.Create(3, 3).Value;

            // Act
            var moveResult = courier.Move(targetLocation);
            moveResult.IsSuccess.Should().BeTrue();
            _courierRepository.Update(courier);

            // Assert
            var courierFromDb = await _courierRepository.GetAsync(courier.Id);
            courierFromDb.HasValue.Should().BeTrue();
            courierFromDb.Value.Should().BeEquivalentTo(courier);
        }

        [Fact]
        public async Task CanGetAllFreeCouriers()
        {
            // Arrange
            var couriers = new List<Courier>()
            {
                Courier.Create("Иван", 2, Location.Create(1, 1).Value).Value,
                Courier.Create("Егор", 1, Location.Create(5, 5).Value).Value,
                Courier.Create("Степан", 3, Location.Create(10, 10).Value).Value
            };
            var orderLocation = Location.Create(5, 5).Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation, 3).Value;

            couriers[2].TakeOrder(order);

            foreach (var c in couriers)
            {
                await _courierRepository.AddAsync(c);
            }
            await _unitOfWork.SaveChangesAsync();

            // Act
            var freeCouriers = await _courierRepository.GetAllFree();

            // Assert
            freeCouriers.Count().Should().Be(2);
            freeCouriers.Should().ContainEquivalentOf(couriers[0]);
            freeCouriers.Should().ContainEquivalentOf(couriers[1]);
        }
    }
}
