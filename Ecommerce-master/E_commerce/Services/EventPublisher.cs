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
        public EventPublisher(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value; 
        }
        public bool Publish(EventModel eventModel)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _settings.HostName,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    Port = _settings.Port
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();


                // declare queue
                channel.QueueDeclare(
                    queue: _settings.Queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // serialize event
                var body = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(eventModel));

                // publish message
                channel.BasicPublish(
                    exchange: _settings.Exchange,
                    routingKey: _settings.Queue,
                    basicProperties: null,
                    body: body
                );

                Console.WriteLine($" [x] Published event: {eventModel}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [!] Error publishing event: {ex.Message}");
                return false;
            }
        }
    }
}
