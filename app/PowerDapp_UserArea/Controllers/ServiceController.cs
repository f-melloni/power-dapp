using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using PowerDapp_UserArea.Database;
using PowerDapp_UserArea.Database.Entity;
using PowerDapp_UserArea.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace PowerDapp_UserArea.Controllers
{
    public class ServiceController : Controller
    {
        private readonly DBEntities _db;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;

        private List<ExchangeRate> exchangeRates;
        private Dictionary<string, List<Order>> ordersByCurrency = new Dictionary<string, List<Order>>();
        private Dictionary<string, List<JToken>> txCache = new Dictionary<string, List<JToken>>();
        
        public ServiceController(DBEntities context, IHostingEnvironment env, IConfiguration configuration)
        {
            _db = context;
            _env = env;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult CheckPayments()
        {
            exchangeRates = _db.ExchangeRates.ToList();

            foreach(var er in exchangeRates) {
                ordersByCurrency[er.CurrencyCode] = _db.Orders.Include(o => o.User).Where(o => o.CurrencyCode == er.CurrencyCode && o.IsPaymentReceived == false). ToList();

                if (ordersByCurrency[er.CurrencyCode].Count > 0) {
                    CacheTransactions(er.CurrencyCode);
                }
            }

            ProcessPayments();
            
            return new EmptyResult();
        }

        [HttpGet]
        public IActionResult CheckStatus(int id)
        {
            OrderAjaxModel result = new OrderAjaxModel();
            Order order = _db.Orders.SingleOrDefault(o => o.Id == id);

            if(order != null) {
                result.IsSuccess = true;
                result.IsPaymentReceived = order.IsPaymentReceived;
                result.Id = order.Id;
                result.Price = order.Price;
                result.TargetWallet = order.TargetWallet;
                result.TokensAmount = order.TokensAmount;
                result.CurrencyCode = order.CurrencyCode;
            }
            else {
                result.IsSuccess = false;
                result.Errors.Add("Requested order was not found");
            }

            return new JsonResult(result);
        }

        #region Payment check methods

        private void ProcessPayments()
        {
            foreach(var er in exchangeRates) {
                if(ordersByCurrency[er.CurrencyCode].Count > 0) {
                    switch(er.CurrencyCode) {
                        case "ETH": ProcessETHPayments(); break;
                        case "BTC": ProcessBTCPayments(); break;
                    }
                }
            }
        }

        private void ProcessETHPayments()
        {
            foreach(JToken tx in txCache["ETH"]) {
                string addressFrom = tx["from"].ToObject<string>().ToLower();
                BigInteger valueInWei = BigInteger.Parse((string)tx["value"]);
                decimal value = Nethereum.Web3.Web3.Convert.FromWei(valueInWei);

                var order = ordersByCurrency["ETH"].Where(o => o.User.ETHWallet.ToLower() == addressFrom && o.Price == value).FirstOrDefault();
                if (order != null) {
                    order.IsPaymentReceived = true;
                }
            }
            _db.SaveChanges();
        }

        private void ProcessBTCPayments()
        {
            string apiBaseUrl = _env.IsDevelopment() ? _configuration["Api:BtcBlockChain:Dev"] : _configuration["Api:BtcBlockChain:Prod"];
            int minConfirmations = exchangeRates.Single(e => e.CurrencyCode == "BTC").MinConfirmations;

            foreach (Order order in ordersByCurrency["BTC"]) {
                string url = string.Format("{0}addr/{1}?noCache=1", apiBaseUrl, order.TargetWallet);
                JToken response = GetResponse(url);

                if(response["balance"].ToObject<decimal>() == order.Price) {
                    // Check confirmation count
                    url = string.Format("{0}tx/{1}", apiBaseUrl, (string)response["transactions"][0]);
                    JToken tx = GetResponse(url);

                    if(tx["confirmations"].ToObject<int>() >= minConfirmations) {
                        order.IsPaymentReceived = true;
                    }
                }
            }
            _db.SaveChanges();
        }

        private void CacheTransactions(string currencyCode)
        {
            txCache[currencyCode] = new List<JToken>();
            switch(currencyCode) {
                case "ETH": CacheETHTransactions(); break;
            }
        }

        private void CacheETHTransactions()
        {
            string apiBaseUrl = _env.IsDevelopment() ? _configuration["Api:EthBlockChain:Dev"] : _configuration["Api:EthBlockChain:Prod"];

            // Get requested confirmation count and wallet address
            var config = exchangeRates.Single(e => e.CurrencyCode == "ETH");
            int minConfirmations = config.MinConfirmations;
            string walletAddress = config.Address;

            // Get current ETH block
            JToken currentBlockResponse = GetResponse(apiBaseUrl + "module=proxy&action=eth_blockNumber");
            int currentBlockNumber = Convert.ToInt32((string)currentBlockResponse["result"], 16);

            // Get ETH block number limits
            var cache = _db.BlockNumberCache.SingleOrDefault(c => c.CurrencyCode == "ETH");
            if(cache == null) {
                cache = new BlockNumberCache() {
                    CurrencyCode = "ETH",
                    BlockNumber = currentBlockNumber - minConfirmations * 2
                };
                _db.BlockNumberCache.Add(cache);
            }
            int startBlockNumber = cache.BlockNumber.Value + 1;
            int endBlockNumber = currentBlockNumber - minConfirmations;

            // Get transactions
            string url = string.Format("{0}module=account&action=txlist&address={1}&startblock={2}&endblock={3}&sort=asc", apiBaseUrl, walletAddress, startBlockNumber, endBlockNumber);
            JToken result = GetResponse(url);

            foreach(JToken tx in result["result"]) {
                if (tx["to"].ToObject<string>().ToLower() == walletAddress.ToLower()) {
                    txCache["ETH"].Add(tx);
                }
            }

            cache.BlockNumber = endBlockNumber;
            _db.SaveChanges();
        }

        private JToken GetResponse(string url, string payload = "", string userName = "", string password = "")
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if(!string.IsNullOrEmpty(userName)) {
                request.Credentials = new NetworkCredential(userName, password);
            }

            request.Method = string.IsNullOrEmpty(payload) ? "GET" : "POST";
            request.ContentType = "application/json";

            if(!string.IsNullOrEmpty(payload)) {
                byte[] postBytes = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = postBytes.Length;
                using(Stream requestStream = request.GetRequestStream()) {
                    requestStream.Write(postBytes, 0, postBytes.Length);
                }
            }

            using (WebResponse response = request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8)) {
                JToken result = JToken.Parse(reader.ReadToEnd());
                return result;
            }
        }

        #endregion
    }
}