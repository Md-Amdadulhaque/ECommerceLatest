using System.Collections.Concurrent;
using System.Text;
using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace E_commerce.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly RabbitMqSettings _settings;
        private readonly ConcurrentBag<IModel> _channelPool;
        private readonly IConnection _connection;
        private readonly int _poolSize;
        public EventPublisher(RabbitMqSettings settings)
        {
            _settings = settings;
            _poolSize = _settings.PoolSize;
            _channelPool = new ConcurrentBag<IModel>();
            var factory = new ConnectionFactory { HostName = _settings.HostName };

            _connection = factory.CreateConnection();

            for (int i = 0; i < _poolSize; i++)
            {
                var channel = _connection.CreateModel();
                _channelPool.Add(channel);
            }
        }
        public void Publish(EventModel eventModel)
        {
            if (!_channelPool.TryTake(out var channel))
            {
                channel = _connection.CreateModel();
            }
            try
            {
                channel.QueueDeclare(queue: eventModel.queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(eventModel.message);

                channel.BasicPublish(exchange: "",
                                     routingKey: eventModel.queueName,
                                     basicProperties: null,
                                     body: body);
            }
            finally
            {
                // Return channel to pool
                _channelPool.Add(channel);
            }
            
        }
    }
}
