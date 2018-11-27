using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Models
{
    public class WalletModel
    {
        [Required]
        [StringLength(42)]
        [RegularExpression("^0x[0-9a-fA-F]{40}$", ErrorMessage = "Please enter valid ethereum address")]
        [Display(Name = "Your ethereum wallet")]
        public string walletAddress { get; set; }
    }
}
