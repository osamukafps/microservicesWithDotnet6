using GeekShopping.OderAPI.Model;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.RabbitMQSender;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _orderRepository;
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMqMessageSender _rabbitMqMessageSender;

        public RabbitMQCheckoutConsumer(OrderRepository repository, IRabbitMqMessageSender rabbitMqMessageSender)
        {
            _orderRepository = repository;
            _rabbitMqMessageSender = rabbitMqMessageSender;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) =>
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                CheckoutHeaderVO checkoutHeaderVO = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
                ProcessOrder(checkoutHeaderVO).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag, false);
            };
            _channel.BasicConsume("checkoutqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO checkoutHeaderVO)
        {
            OrderHeader order = new()
            {
                UserId = checkoutHeaderVO.UserId,
                FirstName = checkoutHeaderVO.FirstName,
                LastName = checkoutHeaderVO.LastName,
                OrderDetails = new List<OrderDetail>(),
                CardNumber = checkoutHeaderVO.CardNumber,
                CouponCode = checkoutHeaderVO.CouponCode,
                CVV = checkoutHeaderVO.CVV,
                DiscountAmount = checkoutHeaderVO.DiscountAmount,
                Email = checkoutHeaderVO.Email,
                ExpiryMonthYear = checkoutHeaderVO.ExpiryMothYear,
                OrderTime = DateTime.Now,
                PurchaseAmount = checkoutHeaderVO.PurchaseAmount,
                PaymentStatus = false,
                Phone = checkoutHeaderVO.Phone,
                DateTime = checkoutHeaderVO.DateTime
            };

            foreach (var details in checkoutHeaderVO.CartDetails)
            {
                OrderDetail detail = new()
                {
                    ProductId = details.ProductId,
                    ProductName = details.Product.Name,
                    Price = details.Product.Price,
                    Count = details.Count,
                };
                order.CartTotalItens += details.Count;
                order.OrderDetails.Add(detail);
            }

            await _orderRepository.AddOrder(order);

            PaymentVO payment = new PaymentVO
            {
                Name = $"{order.FirstName} {order.LastName}",
                CardNumber = order.CardNumber,
                CVV = order.CVV,
                ExpiryMonthYear = order.ExpiryMonthYear,
                OrderId = order.Id,
                PurchaseAmount = order.PurchaseAmount,
                Email = order.Email
            };

            try
            {
                _rabbitMqMessageSender.SendMessage(payment, "orderpaymentprocessqueue");
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
