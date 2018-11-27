using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Models
{
    public class BuyTokensAjaxModel
    {
        [Required]
        public string CurrencyCode { get; set; }

        [Required]
        public int TokensAmount { get; set; }
    }
}
