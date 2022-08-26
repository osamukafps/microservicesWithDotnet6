using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GeekShopping.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService service, ICartService cartService)
        {
            _logger = logger;
            _productService = service;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var productList = await _productService.FindAllProducts("");
            return View(productList);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var product = await _productService.FindProductById(id, token);
            return View(product);
        }

        [Authorize]
        [HttpPost]
        [ActionName("Details")]
        public async Task<IActionResult> DetailsPost(ProductViewModel product)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var cart = new CartViewModel
            {
                CartHeader = new CartHeaderViewModel
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault().Value
                }
            };

            var cartDetail = new CartDetailViewModel
            {
                Count = product.Count,
                ProductId = product.Id,
                Product = await _productService.FindProductById(product.Id, token)
            };

            var cartDetailList = new List<CartDetailViewModel>();
            cartDetailList.Add(cartDetail);
            cart.CartDetails = cartDetailList;

            var response = await _cartService.AddItemToCart(cart, token);
            

            if (response != null)
                return RedirectToAction(nameof(Index));
         
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Logout()
        { 
            return SignOut("Cookies", "oidc");
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return RedirectToAction(nameof(Index));
        }
    }
}