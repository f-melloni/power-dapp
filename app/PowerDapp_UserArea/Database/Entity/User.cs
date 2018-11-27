using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PowerDapp_UserArea.Database.Entity
{
    public partial class User : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [StringLength(60)]
        public string ETHWallet { get; set; }
    }
}
