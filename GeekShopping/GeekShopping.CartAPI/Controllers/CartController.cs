using GeekShopping.CartAPI.Data.ValueObjects;
using GeekShopping.CartAPI.Messages;
using GeekShopping.CartAPI.RabbitMQSender;
using GeekShopping.CartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CartController : ControllerBase
    {
        private ICartRepository _cartRepository;
        private IRabbitMqMessageSender _rabbitMqMessageSender;

        public CartController(ICartRepository cartRepository, IRabbitMqMessageSender rabbitMqMessageSender)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _rabbitMqMessageSender = rabbitMqMessageSender ?? throw new ArgumentNullException(nameof(rabbitMqMessageSender));
        }

        [HttpGet("find-cart/{userId}")]
        public async Task<ActionResult<CartVO>> FindById([FromRoute]string userId)
        {
            var cart = await _cartRepository.FindCartByUserId(userId);

            if (cart == null) 
                return NotFound();

            return Ok(cart);
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO vo)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(vo);

            if (cart == null) 
                return NotFound();

            return Ok(cart);
        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO vo)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(vo);

            if (cart == null) 
                return NotFound();

            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartVO>> RemoveCart(int id)
        {
            var status = await _cartRepository.RemoveFromCart(id);

            if (!status) 
                return BadRequest();

            return Ok(status);
        }

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO vo)
        {
            var status = await _cartRepository.ApplyCoupon(vo.CartHeader.UserId, vo.CartHeader.CouponCode);

            if (!status) 
                return NotFound();

            return Ok(status);
        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> ApplyCoupon(string userId)
        {
            var status = await _cartRepository.RemoveCoupon(userId);

            if (!status) 
                return NotFound();

            return Ok(status);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO checkoutHeaderVO)
        {
            if (checkoutHeaderVO?.UserId == null)
                return BadRequest();

            var cart = await _cartRepository.FindCartByUserId(checkoutHeaderVO.UserId);

            if (cart == null) 
                return NotFound();

            checkoutHeaderVO.CartDetails = cart.CartDetails;
            checkoutHeaderVO.DateTime = DateTime.Now;

            _rabbitMqMessageSender.SendMessage(checkoutHeaderVO, "checkoutqueue");

            return Ok(checkoutHeaderVO);
        }
    }
}