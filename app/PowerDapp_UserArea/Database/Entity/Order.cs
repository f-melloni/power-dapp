using System;
using System.ComponentModel.DataAnnotations;

namespace PowerDapp_UserArea.Database.Entity
{
    public class Order
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        [StringLength(10)]
        public string CurrencyCode { get; set; }

        [Required]
        [StringLength(100)]
        public string TargetWallet { get; set; }

        [Required]
        public int TokensAmount { get; set; }

        [Required]
        public decimal Price { get; set; }

        public bool IsPaymentReceived { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
