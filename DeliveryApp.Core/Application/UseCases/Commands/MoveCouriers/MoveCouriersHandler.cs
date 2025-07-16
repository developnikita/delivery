using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers
{
    public class MoveCouriersHandler : IRequestHandler<MoveCouriersCommand, bool>
    {
        private readonly ICourierRepository _courierRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MoveCouriersHandler(ICourierRepository courierRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
        {
            var couriers = await _courierRepository.GetAllBusy();
            foreach (var courier in couriers)
            {
                var storageWithOrder = courier.StoragePlaces.FirstOrDefault(o => o.OrderId.HasValue);

                if (storageWithOrder != null)
                {
                    var order = await _orderRepository.GetAsync(storageWithOrder.OrderId.Value);
                    if (order.HasValue)
                    {
                        courier.Move(order.Value.Location);
                        if (courier.Location == order.Value.Location)
                        {
                            order.Value.Complete();
                            courier.CompleteOrder(order.Value);
                            _orderRepository.Update(order.Value);
                        }
                        _courierRepository.Update(courier);
                    }
                }
            }

            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
