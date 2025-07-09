using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder
{
    public class AssignOrderHandler : IRequestHandler<AssignOrderCommand, bool>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICourierRepository _courierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDispatchService _dispatchService;

        public AssignOrderHandler(IOrderRepository orderRepository, ICourierRepository courierRepository, IUnitOfWork unitOfWork, IDispatchService dispatchService)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
        }

        public async Task<bool> Handle(AssignOrderCommand request, CancellationToken cancellationToken)
        {
            var freeCouriers = await _courierRepository.GetAllFree();
            if (freeCouriers.Count() == 0)
                return false;

            var unassignedOrder = await _orderRepository.GetFirstInCreatedStatusAsync();
            if (!unassignedOrder.HasValue)
                return false;

            var orderAssignedResult = _dispatchService.Dispatch(unassignedOrder.Value, [.. freeCouriers]);
            if (orderAssignedResult.IsFailure)
                return false;

            _courierRepository.Update(orderAssignedResult.Value);
            _orderRepository.Update(unassignedOrder.Value);

            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
