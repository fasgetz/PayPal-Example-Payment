using Microsoft.Extensions.Configuration;
using PayPal.Api;
using PayPalAspNetCoreTest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayPalAspNetCoreTest.Services
{

    /// <summary>
    /// Сервис, который содержит логику работы с платежной системой PayPal
    /// </summary>
    public class PayPalService : IPayPalService
    {
        /// <summary>
        /// Контекст апи
        /// </summary>
        public APIContext apiContext;

        /// <summary>
        /// Аксесс токен
        /// </summary>
        private string accessToken;


        /// <summary>
        /// Инициализируем контекст апи PayPal
        /// </summary>
        public PayPalService(IConfiguration configSettings)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add("clientId", configSettings.GetValue<string>("PayPalConfig:clientId"));
            config.Add("clientSecret", configSettings.GetValue<string>("PayPalConfig:clientSecret"));

            accessToken = new OAuthTokenCredential(config).GetAccessToken();
            apiContext = new APIContext(accessToken);
        }




        /// <summary>
        /// Создание платежа
        /// </summary>
        /// <param name="products">Список продуктов</param>
        /// <returns></returns>
        public Payment createPayment(IEnumerable<Product> products)
        {
            decimal shippingPrice = 5; // Стоимость отправки


            var items = new List<Item>();
            foreach (var item in products)
            {
                items.Add(new Item()
                {
                    name = item.name,
                    currency = "USD",
                    price = item.price.ToString(),
                    quantity = item.count.ToString(),
                    description = item.description,
                    sku = item.sku // Что-то вроде артикула товара
                });
            }

            var totalSum = (products.Sum(i => i.count * i.price) + shippingPrice).ToString("0.00").Replace(',', '.');
            // Make an API call
            var payment = Payment.Create(apiContext, new Payment
            {
                intent = "sale",                
                payer = new Payer
                {
                    payment_method = "paypal"
                },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        description = "Транзакция может содержать описание",
                        invoice_number = Guid.NewGuid().ToString(), // Номер счета-фактуры всегда должен быть уникальным
                        amount = new Amount
                        {
                            currency = "USD",
                            total = totalSum, //"70.00",
                            details = new Details
                            {
                                tax = "0",                                
                                shipping = shippingPrice.ToString(),
                                subtotal = products.Sum(i => i.count * i.price).ToString() // Общая сумма
                            }
                        },
                        item_list = new ItemList
                        {
                            items = items
                        }
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    return_url = "https://localhost:44339/Home/Success",
                    cancel_url = "https://localhost:44339/cancel"
                }
            });


            return payment;
        }


        /// <summary>
        /// Получение данных платежа по айди
        /// </summary>
        /// <returns></returns>
        public Payment GetPayment(string id)
        {
            return Payment.Get(apiContext, id);
        }


        /// <summary>
        /// Подтверждение платежа
        /// </summary>
        /// <param name="paymentId">Айди платежа</param>
        /// <param name="payerId">Айди плательщика</param>
        /// <param name="transactions">Транзакция</param>
        /// <returns></returns>
        public bool ExecutePayment(string paymentId, string payerId, List<Transaction> transactions)
        {
            try
            {
                PaymentExecution exec = new PaymentExecution();
                exec.payer_id = payerId;
                exec.transactions = transactions;

                Payment.Execute(apiContext, paymentId, exec);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

    }
}
