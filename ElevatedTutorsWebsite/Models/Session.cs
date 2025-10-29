using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutorsWebsite.Models
{
    public class Session
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        public int SessionNumber { get; set; } = 0;// e.g. 3 (current session)

        [Required]
        public DateTime SessionDate { get; set; }

        public enum SessionStatus { Scheduled, Completed, Cancelled }

        [Required]
        public SessionStatus Status { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        [ForeignKey("StudentUser")]
        public int StudentUserId { get; set; }
        public virtual StudentUser StudentUser { get; set; }

        [ForeignKey("TutorUser")]
        public int TutorUserId { get; set; }
        public virtual TutorUser TutorUser { get; set; }

        public Session() { }

        public Session(int sessionId, int sessionNumber, DateTime sessionDate,
                       SessionStatus status, int subjectId, int studentUserId, int tutorUserId)
        {
            SessionId = sessionId;
            SessionNumber = sessionNumber;
            SessionDate = sessionDate;
            Status = status;
            SubjectId = subjectId;
            StudentUserId = studentUserId;
            TutorUserId = tutorUserId;
        }
    }
}
