using GeekShopping.MessageBus;

namespace GeekShopping.OrderAPI.RabbitMQSender
{
    public interface IRabbitMqMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
