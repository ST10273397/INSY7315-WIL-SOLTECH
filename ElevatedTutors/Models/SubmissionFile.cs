using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
{
    public class SubmissionFile
    {
        [Key]
        public int SubmissionFileId { get; set; }

        [Required]
        public string FilePath { get; set; }

        [ForeignKey("Submission")]
        public int SubmissionId { get; set; }
        public virtual Submission Submission { get; set; }
    }
}
