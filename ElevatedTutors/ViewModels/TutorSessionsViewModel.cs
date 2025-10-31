using ElevatedTutors.Models;

namespace ElevatedTutors.ViewModel
{
    public class TutorSessionsViewModel
    {
        public DateTime Date { get; set; }
        public List<Session> Sessions { get; set; } = new();
    }
}