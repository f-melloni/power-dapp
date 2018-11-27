using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PowerDapp_UserArea.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PowerDapp_UserArea.Database.Entity;
using PowerDapp_UserArea.Database;
using NBitcoin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PowerDapp_UserArea.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly DBEntities _db;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _configuration;

        private User _currentUser;
        private User currentUser
        {
            get {
                if(_currentUser == null) {
                    var task = Task.Run(async () => {
                        return await _userManager.GetUserAsync(User);
                    });
                    task.Wait();

                    _currentUser = task.Result;
                }
                return _currentUser;
            }
        }

        public HomeController(UserManager<User> userManager, DBEntities db, IHostingEnvironment env, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _env = env;
            _db = db;
        }

        public IActionResult Dashboard()
        {
            ViewData["TokensPaid"] = _db.Orders.Where(o => o.User.Id == currentUser.Id && o.IsPaymentReceived == true).Sum(o => o.TokensAmount);
            ViewData["TokensNotPaid"] = _db.Orders.Where(o => o.User.Id == currentUser.Id && o.IsPaymentReceived == false).Sum(o => o.TokensAmount);
            ViewData["WalletAddress"] = currentUser.ETHWallet;
            
            return View();
        }

        public IActionResult Disclaimer()
        {
            ViewData["hasTokens"] = _db.Orders.Where(o => o.User.Id == currentUser.Id).Count() > 0;
            return View();
        }
        
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult AddWallet()
        {
            return View();
        }

        public IActionResult BountyProgram()
        {
            return View();
        }

        public IActionResult Payments()
        {
            List<Order> orders = _db.Orders.Where(o => o.User.Id == currentUser.Id).OrderByDescending(o => o.Id).ToList();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWallet(WalletModel model)
        {
            if(ModelState.IsValid) {
                currentUser.ETHWallet = model.walletAddress;

                await _userManager.UpdateAsync(currentUser);

                return RedirectToAction("BuyTokens");
            }
            return View(model);
        }

        public IActionResult BuyTokens()
        {
            ViewData["ExchangeRates"] = _db.ExchangeRates.ToList();

            BuyTokensAjaxModel model = new BuyTokensAjaxModel();
            model.TokensAmount = 1;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BuyTokens(BuyTokensAjaxModel model)
        {
            OrderAjaxModel result = new OrderAjaxModel() {
                IsSuccess = true,
                IsPaymentReceived = false
            };

            try {
                if (ModelState.IsValid) {
                    var exchangeRate = _db.ExchangeRates.SingleOrDefault(e => e.CurrencyCode == model.CurrencyCode);

                    if (exchangeRate != null) {
                        string targetWallet = !string.IsNullOrEmpty(exchangeRate.Address)
                            ? exchangeRate.Address
                            : GetTargetWallet(model.CurrencyCode);

                        Order order = new Order() {
                            CurrencyCode = model.CurrencyCode,
                            TokensAmount = model.TokensAmount,
                            Price = Decimal.Multiply(exchangeRate.Rate, model.TokensAmount),
                            TargetWallet = targetWallet,
                            User = currentUser,
                            IsPaymentReceived = false,
                        };
                        
                        _db.Orders.Add(order);
                        _db.SaveChanges();

                        result.Id = order.Id;
                        result.Price = order.Price;
                        result.TargetWallet = order.TargetWallet;
                        result.TokensAmount = order.TokensAmount;
                        result.CurrencyCode = order.CurrencyCode;
                    }
                }
            }
            catch(Exception e) {
                result.IsSuccess = false;
                result.Errors.Add(e.Message);
            }

            return new JsonResult(result);
        }

        private string GetTargetWallet(string currencyCode)
        {
            switch(currencyCode) {
                case "BTC": {
                        uint index = (uint)_db.Orders.Where(o => o.CurrencyCode == "BTC").Count();
                        string xpub = _env.IsDevelopment() ? _configuration["XPubKeys:Dev:BTC"] : _configuration["XPubKeys:Prod:BTC"];

                        var pubKey = ExtPubKey.Parse(xpub);
                        var address = pubKey.Derive(0).Derive(index).PubKey.GetSegwitAddress(_env.IsDevelopment() ? Network.TestNet : Network.Main);

                        return address.ToString();
                    };
                default: throw new Exception("Unsupported currency");
            }
        }
    }
}
