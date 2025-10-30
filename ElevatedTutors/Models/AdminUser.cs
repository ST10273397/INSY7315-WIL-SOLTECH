using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
{
    public class AdminUser
    {
        [Key, ForeignKey("User")]
        public string? UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public AdminUser() { }

        public AdminUser(string? user)
        {
            UserId = user;
        }
    }
}
