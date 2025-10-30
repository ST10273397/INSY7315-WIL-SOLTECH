using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutors.Models
{
    public class TutorUser
    {
        [Key, ForeignKey("User")]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual List<Subject> Subjects { get; set; } = new();
        public virtual List<Session> Sessions { get; set; } = new();
        public TutorUser() { }
        public TutorUser(string userId, List<Subject> subjects, List<Session> sessions) 
        {
            UserId = userId;
            Subjects = subjects;
            Sessions = sessions;
        }
    }

}
