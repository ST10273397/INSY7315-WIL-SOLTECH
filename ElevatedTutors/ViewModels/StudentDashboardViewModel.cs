using ElevatedTutors.Models;

namespace ElevatedTutors.ViewModel
{
    public class StudentDashboardViewModel
    {
        public List<Subject> Classes { get; set; }
        public List<Submission> DueDates { get; set; }

        public List<string> Announcements { get; set; } = new List<string>();

    }
}
