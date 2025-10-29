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

        public virtual List<Subject> Subjects { get; set; } = new();

        // Private backing field for sessions
        private readonly List<Session> _sessions = new();
        public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();

        public int MaxSessions { get; set; } = 10;

        public virtual List<Submission> Submissions { get; set; } = new();

        public StudentUser() { }

        public StudentUser(int userId, string parentEmail, decimal marks, List<Subject> subjects, List<Session> sessions, int maxSessions, List<Submission> submissions)
        {
            UserId = userId;
            ParentEmail = parentEmail;
            Marks = marks;
            Subjects = subjects;
            _sessions.AddRange(sessions);
            MaxSessions = maxSessions;
            Submissions = submissions;
        }

    }
}
