using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace ElevatedTutorsWebsite.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }

        [Required]
        public string SubjectName { get; set; }

        public string SubjectDesc { get; set; }

        public int? TutorUserId { get; set; }
        public virtual TutorUser TutorUser { get; set; }

        public int? StudentUserId { get; set; }
        public virtual StudentUser StudentUser { get; set; }

        public virtual List<Session> Sessions { get; set; } = new();

        public virtual List<Submission> Submissions { get; set; }

        public Subject() { }

        public Subject(int subjectId, string subjectName, string subjectDesc, int studentUserId, int tutorUserId, List<Session> sessions, List<Submission> submissions)
        {
            SubjectId = subjectId;
            SubjectName = subjectName;
            SubjectDesc = subjectDesc;
            StudentUserId = studentUserId;
            TutorUserId = tutorUserId;
            Sessions = sessions;
            Submissions = submissions;
        }
    }    
}
