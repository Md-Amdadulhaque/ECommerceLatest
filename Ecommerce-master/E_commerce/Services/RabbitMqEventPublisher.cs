using E_commerce.Interface;
using MongoDB.Bson.IO;
using System.Text;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;

    public RabbitMqEventPublisher(IConnection connection)
    {
        _connection = connection;
    }

    public void Publish<T>(T @event) where T : class
    {
        using var channel = _connection.CreateModel();
        channel.QueueDeclare("email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        channel.BasicPublish("", "email_queue", null, messageBody);
    }
}