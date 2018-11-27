using PowerDapp_UserArea.Database.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Models
{
    public class OrderAjaxModel
    {
        public int Id { get; set; }
        public string CurrencyCode { get; set; }
        public string TargetWallet { get; set; }
        public int TokensAmount { get; set; }
        public decimal Price { get; set; }
        public bool IsPaymentReceived { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> Errors { get; set; }

        public OrderAjaxModel()
        {
            Errors = new List<string>();
        }
    }
}
