using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier
{
    public class CreateCourierHandler : IRequestHandler<CreateCourierCommand, bool>
    {
        private readonly ICourierRepository _courierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCourierHandler(ICourierRepository courierRepository, IUnitOfWork unitOfWork)
        {
            _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
        {
            var courierCreatedResult = Courier.Create(request.Name, request.Speed, Location.CreateRandom());
            if (courierCreatedResult.IsFailure)
                return false;

            await _courierRepository.AddAsync(courierCreatedResult.Value);
            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
