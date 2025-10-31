using ElevatedTutors.Models;
using System.Collections.Generic;

namespace ElevatedTutors.Models
{
    public class StudentScheduleViewModel
    {
        public List<Session> Sessions { get; set; } = new();
        public List<Submission> DueDates { get; set; } = new();
    }
}
