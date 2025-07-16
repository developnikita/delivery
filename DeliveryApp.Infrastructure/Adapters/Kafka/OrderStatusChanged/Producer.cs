using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderStatusChanged;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.OrderStatusChanged
{
    public class Producer : IMessageBusProducer
    {
        private readonly ProducerConfig _config;
        private readonly string _topicName;

        public Producer(IOptions<Settings> options)
        {
            if (string.IsNullOrEmpty(options.Value.MessageBrokerHost))
                throw new ArgumentException(nameof(options.Value.MessageBrokerHost));

            if (string.IsNullOrEmpty(options.Value.OrderStatusChangedTopic))
                throw new ArgumentException(nameof(options.Value.OrderStatusChangedTopic));

            _config = new ProducerConfig
            {
                BootstrapServers = options.Value.MessageBrokerHost
            };
            _topicName = options.Value.OrderStatusChangedTopic;
        }

        public async Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            var orderCompletedIntegrationEvent = new OrderStatusChangedIntegrationEvent()
            {
                OrderId = notification.OrderId.ToString(),
                OrderStatus = OrderStatus.Completed
            };

            var message = new Message<string, string>
            {
                Key = notification.OrderId.ToString(),
                Value = JsonConvert.SerializeObject(orderCompletedIntegrationEvent)
            };

            using var producer = new ProducerBuilder<string, string>(_config).Build();

            await producer.ProduceAsync(_topicName, message, cancellationToken);

        }
    }
}
