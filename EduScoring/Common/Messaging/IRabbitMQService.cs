namespace EduScoring.Common.Messaging;

public interface IRabbitMQService
{
    Task PublishAsync<T>(string queueName, T message);
}