using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatedTutorsWebsite.Models
{
    public class TutorUser
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual List<Subject> Subjects { get; set; } = new();
        public virtual List<Session> Sessions { get; set; } = new();
        public TutorUser() { }
        public TutorUser(int userId, List<Subject> subjects, List<Session> sessions) 
        {
            UserId = userId;
            Subjects = subjects;
            Sessions = sessions;
        }
    }

}
