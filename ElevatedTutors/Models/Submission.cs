using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Notes { get; set; }

        public DateTime UploadDate { get; set; }

        public decimal Grade { get; set; }

        public string Status { get; set; } 

        public virtual List<SubmissionFile> Files { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        [ForeignKey("StudentUser")]
        public string? UserId { get; set; }
        public virtual StudentUser StudentUser { get; set; }

        public Submission() { }

        public Submission(int submissionId, string title, string notes, DateTime uploadDate, decimal grade, int subjectId, string userId)
        {
            SubmissionId = submissionId;
            Title = title;
            Notes = notes;
            UploadDate = uploadDate;
            Grade = grade;
            SubjectId = subjectId;
            UserId = userId;
        }
    }
}
