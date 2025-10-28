using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutorsWebsite.Models
{
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Notes { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; }

        public decimal Grade { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; }

        [ForeignKey("StudentUser")]
        public int UserId { get; set; }
        public virtual StudentUser StudentUser { get; set; }

        public Submission() { }

        public Submission(int submissionId, string title, string notes, string filePath, DateTime uploadDate, decimal grade, int subjectId, int userId)
        {
            SubmissionId = submissionId;
            Title = title;
            Notes = notes;
            FilePath = filePath;
            UploadDate = uploadDate;
            Grade = grade;
            SubjectId = subjectId;
            UserId = userId;
        }
    }
}
