﻿using GeekShopping.MessageBus;

namespace GeekShopping.CartAPI.RabbitMQSender
{
    public interface IRabbitMqMessageSender
    {
        void SendMessage(BaseMessage baseMessage, string queueName);
    }
}
