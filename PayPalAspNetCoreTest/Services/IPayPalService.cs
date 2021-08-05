using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayPal.Api;
using PayPalAspNetCoreTest.Models;

namespace PayPalAspNetCoreTest.Services
{
    public interface IPayPalService
    {

        /// <summary>
        /// Подтверждение платежа
        /// </summary>
        /// <param name="paymentId">Айди платежа</param>
        /// <param name="payerId">Айди плательщика</param>
        /// <param name="transactions">Транзакция</param>
        /// <returns></returns>
        public bool ExecutePayment(string paymentId, string payerId, List<Transaction> transactions);

        /// <summary>
        /// Создание платежа
        /// </summary>
        /// <param name="products">Список продуктов</param>
        /// <returns></returns>
        public Payment createPayment(IEnumerable<Product> products);

        /// <summary>
        /// Получение данных платежа по айди
        /// </summary>
        /// <returns></returns>
        public Payment GetPayment(string id);
    }
}
