using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories
{
    public class CourierRepository : ICourierRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CourierRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(Courier courier)
        {
            await _dbContext.Couriers.AddAsync(courier);
        }

        public async Task<IEnumerable<Courier>> GetAllFree()
        {
            var couriers = await _dbContext.Couriers
                                     .Include(x => x.StoragePlaces)
                                     .Where(o => o.StoragePlaces.All(c => c.OrderId == null))
                                     .ToArrayAsync();
            return couriers;
        }

        public async Task<Maybe<Courier>> GetAsync(Guid courierId)
        {
            var courier = await _dbContext.Couriers
                                          .Include(x => x.StoragePlaces)
                                          .SingleOrDefaultAsync(o => o.Id == courierId);
            return courier;

        }

        public void Update(Courier courier)
        {
            _dbContext.Couriers.Update(courier);
        }
    }
}
