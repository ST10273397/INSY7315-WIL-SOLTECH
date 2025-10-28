using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ElevatedTutorsWebsite.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } // "Student", "Tutor", or "Admin"

        public User() { }

        public User(int userId, string firstName, string surname, string email, string password, string role)
        {
            UserId = userId;
            FirstName = firstName;
            Surname = surname;
            Email = email;
            Password = password;
            Role = role;
        }
    }
}
