using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Identity fields (Id, Email, PasswordHash, UserName, etc) come from IdentityUser

        public string? FirstName { get; set; }
        public string? Surname { get; set; }

        public string? Role { get; set; }  

        // Approval flag
        public bool IsApproved { get; set; } = false;

        // For display only; not mapped to DB 
        [NotMapped]
        public List<string> Roles { get; set; } = new List<string>();
    }
}
