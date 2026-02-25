using System.Text;
using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace E_commerce.Services
{
    public class EventConsumer : IEventConsumer
    {
        private readonly RabbitMqSettings _settings;
        public EventConsumer(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;
        }
        public void Listner()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                Port = _settings.Port
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                   queue: _settings.Queue,
                   durable: true,
                   exclusive: false,
                   autoDelete: false,
                   arguments: null
               );
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventModel) =>
            {
                var body = eventModel.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Gets {message}");
            };

            channel.BasicConsume(
                queue: _settings.Queue,
                consumer: consumer
             );
        }
    }
}
