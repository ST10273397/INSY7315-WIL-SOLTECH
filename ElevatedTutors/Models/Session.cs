using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
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
        public string StudentUserId { get; set; }
        public virtual StudentUser StudentUser { get; set; }

        [ForeignKey("TutorUser")]
        public string TutorUserId { get; set; }
        public virtual TutorUser TutorUser { get; set; }

        public Session() { }

        public Session(int sessionId, int sessionNumber, DateTime sessionDate, SessionStatus status, int subjectId, string studentUserId, string tutorUserId)
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
