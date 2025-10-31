using ElevatedTutors.Models;
using System;
using System.Collections.Generic;

namespace ElevatedTutors.ViewModel
{
    public class TutorDashboardViewModel
    {
        public Session CurrentSession { get; set; }
        public List<IGrouping<DateTime, Session>> UpcomingSessionsGrouped { get; set; }
        public List<StudentUser> Students { get; set; }
        public int DefaultSessionLengthMinutes { get; set; } = 60; // default is an hour
    }
}