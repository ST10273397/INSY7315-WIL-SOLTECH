namespace ElevatedTutors.Models
{
    public class TutorSubmissionViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentUserId { get; set; }    
        public string StudentFullName { get; set; }  
        public string Title { get; set; }
        public string SubjectName { get; set; }
        public int FileCount { get; set; }
        public decimal Grade { get; set; }
        public string Status { get; set; }           
        public DateTime UploadDate { get; set; }
        public string Notes { get; set; }            
    }
}
