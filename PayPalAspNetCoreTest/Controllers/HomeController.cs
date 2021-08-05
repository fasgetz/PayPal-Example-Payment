using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPal.Api;
using PayPalAspNetCoreTest.Models;
using PayPalAspNetCoreTest.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalAspNetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        // Формируем список продуктов
        List<Product> products = new List<Product>()
            {
                new Product()
                {
                    name = "Milk",
                    count = 5,
                    price = 1,
                    sku = "product",
                    description = "Молочный продукт 3%"
                },
                new Product()
                {
                    name = "Chicken",
                    count = 3,
                    price = 20,
                    sku = "Meat",
                    description = "Курица 900 г."
                },
                new Product()
                {
                    name = "Apple",
                    count = 30,
                    price = 1,
                    sku = "Fruit",
                    description = "Зеленое яблоко"
                },
            };

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configSettings;
        private readonly IMemoryCache cache;
        private readonly IPayPalService PayPalService;

        public HomeController(ILogger<HomeController> logger, IConfiguration configSettings, IMemoryCache cache, IPayPalService PayPalService)
        {
            _logger = logger;
            this.configSettings = configSettings;
            this.cache = cache;
            this.PayPalService = PayPalService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {


            return View(products);

            //// Создание платежа
            //var payment = PayPalService.createPayment(products);

            //// Получение данных созданного платежа
            //var getPayment = PayPalService.GetPayment(payment.id);


            //// Получаем ссылку для редиректа на страницу оплаты PayPal
            //var url = getPayment.links.FirstOrDefault(i => i.method == "REDIRECT").href;



            //return Redirect(url);
        }


        public IActionResult PaymentPayPal()
        {

            // Создание платежа
            var payment = PayPalService.createPayment(products);

            // Получение данных созданного платежа
            var getPayment = PayPalService.GetPayment(payment.id);


            // Получаем ссылку для редиректа на страницу оплаты PayPal
            var url = getPayment.links.FirstOrDefault(i => i.method == "REDIRECT").href;

            // payment.id = айди транзакции, который необходимо кэшировать, чтобы потом извлечь
            cache.Set(payment.id, getPayment.transactions, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60) });


            return Redirect(url);

            //return View();
        }

        public IActionResult Success(string PayerID, string paymentId, string token)
        {
            List<Transaction> transactions = cache.Get<List<Transaction>>(paymentId);

            bool success = PayPalService.ExecutePayment(paymentId, PayerID, transactions);


            return Ok("Success Payment!");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
