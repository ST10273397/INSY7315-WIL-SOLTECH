using ElevatedTutors.Data;
using ElevatedTutors.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElevatedTutors.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            //  var studentUserId = _userManager.GetUserId(User);
            string studentUserId = "stu-001";  // hardcoded for now

            var student = await _context.StudentUsers
                .Include(s => s.Subjects)
                    .ThenInclude(sub => sub.TutorUser)
                        .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(s => s.UserId == studentUserId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var viewModel = new StudentDashboardViewModel
            {
                Classes = student.Subjects.ToList(),
                DueDates = await _context.Submissions
                    .Include(sub => sub.Subject)
                    .Where(sub => sub.UserId == studentUserId)
                    .OrderBy(sub => sub.UploadDate)
                    .ToListAsync(),
                Announcements = new List<string>
                {
                    "Welcome to the new term!",
                    "Submit your assignments before the due date.",
                    "Math workshop next Friday."
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Classes()
        {
            //  var studentUserId = _userManager.GetUserId(User);
            string studentUserId = "stu-001";

            var subjects = await _context.Subjects
                .Include(s => s.TutorUser)
                    .ThenInclude(t => t.User)
                .Where(s => s.StudentUserId == studentUserId)
                .ToListAsync();

            var viewModel = new StudentClassesViewModel
            {
                Classes = subjects
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Schedule()
        {
            //  var studentUserId = _userManager.GetUserId(User);
            string studentUserId = "stu-001";

            // Get all sessions for the student
            var sessions = await _context.Sessions
                .Include(s => s.Subject)
                .Include(s => s.TutorUser)
                .ThenInclude(t => t.User)
                .Where(s => s.StudentUserId == studentUserId)
                .ToListAsync();

            // Upcoming submissions
            var dueDates = await _context.Submissions
                .Include(sub => sub.Subject)
                .Where(sub => sub.UserId == studentUserId)
                .OrderBy(sub => sub.UploadDate)
                .ToListAsync();

            var viewModel = new StudentScheduleViewModel
            {
                Sessions = sessions,
                DueDates = dueDates
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Report()
        {
            //  var studentUserId = _userManager.GetUserId(User);
            string studentUserId = "stu-001";

            var submissions = await _context.Submissions
                .Include(s => s.Subject)
                .Include(s => s.Files)
                .Where(s => s.UserId == studentUserId)
                .OrderByDescending(s => s.UploadDate)
                .ToListAsync();

            var viewModel = submissions.Select(s => new ReportViewModel
            {
                SubmissionId = s.SubmissionId,
                Title = s.Title,
                SubjectName = s.Subject.SubjectName,
                FileCount = s.Files?.Count ?? 0,
                Grade = s.Grade,
                Status = s.Status,
                Notes = s.Notes
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Feedback(int id)
        {
            var submission = await _context.Submissions
                .FirstOrDefaultAsync(s => s.SubmissionId == id);

            if (submission == null)
                return NotFound();

            
            return View(submission);
        }

        public IActionResult ContactUs()
        {
            return View();
        }
    }
}