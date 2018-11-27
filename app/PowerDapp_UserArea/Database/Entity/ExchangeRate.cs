using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Database.Entity
{
    public class ExchangeRate
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CurrencyCode { get; set; }

        [Required]
        public decimal Rate { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        public int MinConfirmations { get; set; }
    }
}
