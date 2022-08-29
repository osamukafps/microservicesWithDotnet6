using GeekShopping.MessageBus;

namespace GeekShopping.PaymentAPI.RabbitMQSender
{
    public interface IRabbitMqMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
