using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<bool>
    {
        public CreateOrderCommand(Guid baskerId, string street, int volume)
        {
            BasketId = baskerId != Guid.Empty
                       ? baskerId
                       : throw new ArgumentException(nameof(baskerId));

            Street = !string.IsNullOrEmpty(street)
                     ? street
                     : throw new ArgumentException(nameof(street));

            Volume = volume > 0
                     ? volume
                     : throw new ArgumentException(nameof(volume));
        }

        public Guid BasketId { get; }
        public string Street { get; }
        public int Volume { get; }
    }
}
