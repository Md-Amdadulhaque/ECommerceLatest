using E_commerce.Interface;
using E_commerce.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using static MongoDB.Driver.WriteConcern;

public class EmailConsumerService : IEmailConsumerService
{
    private readonly RabbitMQ.Client.IConnection _connection;
    private readonly IModel _channel;
    private readonly string _apiKey;
    private readonly string _mailSender;

    public EmailConsumerService(RabbitMQ.Client.IConnection connection,IConfiguration configuration)
    {
        _connection = connection;
      //  _channel = _connection.CreateModel();
    //    _channel.QueueDeclare("email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        _apiKey = configuration["SendGrid:ApiKey"];
        _mailSender = configuration["SendGrid:MailSender"];
    }

    //protected override Task ExecuteAsync(CancellationToken stoppingToken)
    //{
    //    var consumer = new EventingBasicConsumer(_channel);
    //    consumer.Received += (model, ea) =>
    //    {
    //        var body = ea.Body.ToArray();
    //        var message = Encoding.UTF8.GetString(body);

    //        var userEvent = JsonConvert.DeserializeObject<UserRegisteredEvent>(message);

    //        // Send email
    //        SendEmail(userEvent);

    //        _channel.BasicAck(ea.DeliveryTag, false);
    //    };

    //    _channel.BasicConsume("email_queue", false, consumer);
    //    return Task.CompletedTask;
    //}

    //private void SendEmail(MailRequest mailRequest)
    //{
    //  //create a event and pass to rabbitmq i guess!!!
    //}

    //public override void Dispose()
    //{
    //    _channel.Close();
    //    _connection.Close();
    //    base.Dispose();
    //}
}
