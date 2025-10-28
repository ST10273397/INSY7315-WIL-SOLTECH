using Microsoft.Extensions.Configuration.UserSecrets;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutorsWebsite.Models
{
    public class StudentUser
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public string ParentEmail { get; set; }

        public decimal Marks { get; set; }

        public virtual List<Subject> Subjects { get; set; }

        public virtual List<Session> Sessions { get; set; } = new();

        public virtual List<Submission> Submissions { get; set; } = new();

        public StudentUser() { }

        public StudentUser(int userId, string parentEmail, decimal marks, List<Subject> subjects, List<Session> sessions, List<Submission> submissions)
        {
            UserId = userId;
            ParentEmail = parentEmail;
            Marks = marks;
            Subjects = subjects;
            Sessions = sessions;
            Submissions = submissions;
        }
    }


}
