using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutorsWebsite.Models
{
    public class AdminUser
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
