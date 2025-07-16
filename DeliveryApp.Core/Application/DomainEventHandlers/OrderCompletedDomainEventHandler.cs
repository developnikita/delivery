using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers
{
    public class OrderCompletedDomainEventHandler : INotificationHandler<OrderCompletedDomainEvent>
    {
        private readonly IMessageBusProducer _messageBusProducer;

        public OrderCompletedDomainEventHandler(IMessageBusProducer messageBusProducer)
        {
            _messageBusProducer = messageBusProducer;
        }

        public async Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            await _messageBusProducer.Publish(notification, cancellationToken);
        }
    }
}
