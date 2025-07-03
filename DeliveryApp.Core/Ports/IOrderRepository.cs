using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task AddAsync(Order order);

        void Update(Order order);

        Task<Maybe<Order>> GetAsync(Guid orderId);

        Task<Maybe<Order>> GetFirstInCreatedStatusAsync();

        IEnumerable<Order> GetAllInAssignedStatus();
    }
}
