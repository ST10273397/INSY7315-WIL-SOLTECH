using ElevatedTutors.Data;
using ElevatedTutors.Models;
using ElevatedTutors.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElevatedTutors.Controllers
{
    public class TutorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public TutorController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Dashboard()
        {
            //  var tutorUserId = _userManager.GetUserId(User);
            string tutorId = "tut-001"; // hardcoded for now

            
            var sessions = await _context.Sessions
                .Include(s => s.StudentUser)
                    .ThenInclude(su => su.User)
                .Include(s => s.Subject)
                .Include(s => s.TutorUser)
                    .ThenInclude(tu => tu.User)
                .Where(s => s.TutorUserId == tutorId)
                .OrderBy(s => s.SessionDate)
                .ToListAsync();

            // get current session 
            var currentSession = sessions.FirstOrDefault(s => s.Status == Session.SessionStatus.Scheduled);

            // group other sessions by date 
            var upcomingSessionsGrouped = sessions
                .Where(s => s != currentSession)
                .GroupBy(s => s.SessionDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            
            var students = await _context.StudentUsers
                .Include(su => su.User)
                .Include(su => su.Subjects)
                .Where(su => su.Subjects.Any(sub => sub.TutorUserId == tutorId))
                .ToListAsync();

            var viewModel = new TutorDashboardViewModel
            {
                CurrentSession = currentSession,
                UpcomingSessionsGrouped = upcomingSessionsGrouped,
                Students = students
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> StartSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            
            TempData["Message"] = $"Session #{session.SessionNumber} started.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> EndSession(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            session.Status = Session.SessionStatus.Completed;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Session #{session.SessionNumber} ended.";
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Sessions()
        {
            //  var tutorUserId = _userManager.GetUserId(User);
            var tutorUserId = "tut-001";

            if (tutorUserId == null)
                return Unauthorized();

          
            var sessions = await _context.Sessions
                .Include(s => s.StudentUser)
                    .ThenInclude(stu => stu.User)
                .Include(s => s.Subject)
                .Where(s => s.TutorUserId == tutorUserId)
                .OrderBy(s => s.SessionDate)
                .ToListAsync();

            
            var groupedSessions = sessions
                .GroupBy(s => s.SessionDate.Date)
                .Select(g => new TutorSessionsViewModel
                {
                    Date = g.Key,
                    Sessions = g.ToList()
                })
                .OrderBy(g => g.Date)
                .ToList();

            return View(groupedSessions);
        }

        public IActionResult Calendar()
        {
            return View();
        }

        public async Task<IActionResult> Submissions()
        {
            //  var tutorUserId = _userManager.GetUserId(User);
            string tutorUserId = "tut-001";

            var studentSubmissions = await _context.StudentUsers
                .Include(s => s.User)
                .Include(s => s.Subjects)
                    .ThenInclude(sub => sub.TutorUser)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Files)
                .Include(s => s.Submissions)
                    .ThenInclude(sub => sub.Subject)
                .Where(s => s.Subjects.Any(sub => sub.TutorUser.UserId == tutorUserId)) // only students assigned to this tutor
                .Select(s => new TutorSubmissionViewModel
                {
                    StudentUserId = s.UserId,
                    StudentFullName = s.User.FirstName + " " + s.User.Surname,

                    // get latest submission if any
                    Title = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Title
                        : "No submission",

                    SubjectName = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Subject.SubjectName
                        : "N/A",

                    FileCount = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Files.Count
                        : 0,

                    Grade = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Grade
                        : 0,

                    Status = s.Submissions.Any()
                        ? (s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Status ?? "Pending")
                        : "No submission",

                    UploadDate = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().UploadDate
                        : DateTime.MinValue,

                    Notes = s.Submissions.Any()
                        ? s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Notes
                        : null
                })
                .OrderBy(r => r.StudentFullName)
                .ToListAsync();

            return View(studentSubmissions);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeedback(int submissionId, string feedback, decimal? grade)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) return NotFound();

            submission.Notes = feedback ?? submission.Notes;
            if (grade.HasValue) submission.Grade = grade.Value;

           
            submission.Status = string.IsNullOrWhiteSpace(submission.Notes) ? submission.Status : "Graded";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Submissions));
        }

        
        public async Task<IActionResult> EditSubmission(int id)
        {
            var submission = await _context.Submissions
                .Include(s => s.Files)
                .Include(s => s.StudentUser).ThenInclude(su => su.User)
                .Include(s => s.Subject)
                .FirstOrDefaultAsync(s => s.SubmissionId == id);

            if (submission == null) return NotFound();

            
            return View(submission);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMarks(int submissionId, decimal grade)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) return NotFound();

            submission.Grade = grade;
            submission.Status = "Complete"; 
            await _context.SaveChangesAsync();

            TempData["Message"] = "Marks updated successfully.";
            return RedirectToAction("TaskSubmissions");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFeedback(int submissionId, string notes)
        {
            var submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null) return NotFound();

            submission.Notes = notes;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Feedback saved successfully.";
            return RedirectToAction("TaskSubmissions");
        }

        public IActionResult Planner()
        {
            return View();
        }
            
    }
}
