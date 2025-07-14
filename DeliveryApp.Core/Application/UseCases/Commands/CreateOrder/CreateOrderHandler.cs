using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeoClient _geoService;

        public CreateOrderHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IGeoClient geoService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _geoService = geoService;
        }

        public async Task<bool> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // NOTE: Не получаем вес заказа из команды.
            var location = await _geoService.GetLocationAsync(request.Street, cancellationToken);
            if (location.IsFailure)
                return false;
            var orderCreatedResult = Order.Create(request.BasketId, location.Value, 1);
            if (orderCreatedResult.IsFailure)
                return false;

            await _orderRepository.AddAsync(orderCreatedResult.Value);
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
