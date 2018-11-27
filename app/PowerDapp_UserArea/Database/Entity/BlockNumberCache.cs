using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Database.Entity
{
    public class BlockNumberCache
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string CurrencyCode { get; set; }

        public int? BlockNumber { get; set; }
        
        [StringLength(100)]
        public string BlockHash { get; set; }
    }
}
