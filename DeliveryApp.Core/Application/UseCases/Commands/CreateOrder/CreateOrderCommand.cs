using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<bool>
    {
        public CreateOrderCommand(Guid baskerId, string street)
        {
            BasketId = baskerId != Guid.Empty
                       ? baskerId
                       : throw new ArgumentException(nameof(baskerId));

            Street = !string.IsNullOrEmpty(street)
                     ? street
                     : throw new ArgumentException(nameof(street));
        }

        public Guid BasketId { get; }
        public string Street { get; }
    }
}
