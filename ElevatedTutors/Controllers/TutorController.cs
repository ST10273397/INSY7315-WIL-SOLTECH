using ElevatedTutors.Data;
using ElevatedTutors.Models;
using ElevatedTutors.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElevatedTutors.Controllers
{
    [Authorize(Roles = "Tutor")] // ✅ Restrict access to only users with the "Tutor" role
    public class TutorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TutorController> _logger; // ✅ Logging for debugging & traceability

        public TutorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<TutorController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Displays the tutor dashboard showing current and upcoming sessions, and associated students.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var tutorUserId = _userManager.GetUserId(User);
                if (tutorUserId == null)
                    return Unauthorized("User is not logged in.");

                var sessions = await _context.Sessions
                    .Include(s => s.StudentUser).ThenInclude(su => su.User)
                    .Include(s => s.Subject)
                    .Include(s => s.TutorUser).ThenInclude(tu => tu.User)
                    .Where(s => s.TutorUserId == tutorUserId)
                    .OrderBy(s => s.SessionDate)
                    .ToListAsync();

                var currentSession = sessions.FirstOrDefault(s => s.Status == Session.SessionStatus.Scheduled);

                var upcomingSessionsGrouped = sessions
                    .Where(s => s != currentSession)
                    .GroupBy(s => s.SessionDate.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                var students = await _context.StudentUsers
                    .Include(su => su.User)
                    .Include(su => su.Subjects)
                    .Where(su => su.Subjects.Any(sub => sub.TutorUserId == tutorUserId))
                    .ToListAsync();

                var viewModel = new TutorDashboardViewModel
                {
                    CurrentSession = currentSession,
                    UpcomingSessionsGrouped = upcomingSessionsGrouped,
                    Students = students
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Tutor Dashboard.");
                return StatusCode(500, "An unexpected error occurred while loading the dashboard.");
            }
        }

        /// <summary>
        /// Marks a tutoring session as started.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartSession(int id)
        {
            try
            {
                var session = await _context.Sessions.FindAsync(id);
                if (session == null) return NotFound("Session not found.");

                var tutorUserId = _userManager.GetUserId(User);
                if (session.TutorUserId != tutorUserId)
                    return Forbid("You are not authorized to modify this session.");

                session.Status = Session.SessionStatus.InProgress;
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Session #{session.SessionNumber} started.";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting session {SessionId}", id);
                return StatusCode(500, "An error occurred while starting the session.");
            }
        }

        /// <summary>
        /// Marks a tutoring session as completed.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EndSession(int id)
        {
            try
            {
                var session = await _context.Sessions.FindAsync(id);
                if (session == null) return NotFound("Session not found.");

                var tutorUserId = _userManager.GetUserId(User);
                if (session.TutorUserId != tutorUserId)
                    return Forbid("You are not authorized to end this session.");

                session.Status = Session.SessionStatus.Completed;
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Session #{session.SessionNumber} ended.";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending session {SessionId}", id);
                return StatusCode(500, "An error occurred while ending the session.");
            }
        }

        /// <summary>
        /// Lists all tutoring sessions for the current tutor, grouped by date.
        /// </summary>
        public async Task<IActionResult> Sessions()
        {
            try
            {
                var tutorUserId = _userManager.GetUserId(User);
                if (tutorUserId == null) return Unauthorized();

                var sessions = await _context.Sessions
                    .Include(s => s.StudentUser).ThenInclude(stu => stu.User)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tutor sessions.");
                return StatusCode(500, "Unable to load sessions.");
            }
        }

        public IActionResult Calendar()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading calendar view.");
                return StatusCode(500, "Unable to load calendar.");
            }
        }

        /// <summary>
        /// Displays recent student submissions assigned to the tutor.
        /// </summary>
        public async Task<IActionResult> Submissions()
        {
            try
            {
                var tutorUserId = _userManager.GetUserId(User);
                if (tutorUserId == null)
                    return Unauthorized();

                var studentSubmissions = await _context.StudentUsers
                    .Include(s => s.User)
                    .Include(s => s.Subjects).ThenInclude(sub => sub.TutorUser)
                    .Include(s => s.Submissions).ThenInclude(sub => sub.Files)
                    .Include(s => s.Submissions).ThenInclude(sub => sub.Subject)
                    .Where(s => s.Subjects.Any(sub => sub.TutorUser.UserId == tutorUserId))
                    .Select(s => new TutorSubmissionViewModel
                    {
                        StudentUserId = s.UserId,
                        StudentFullName = s.User.FirstName + " " + s.User.Surname,
                        Title = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Title ?? "No submission",
                        SubjectName = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault().Subject?.SubjectName ?? "N/A",
                        FileCount = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault()?.Files?.Count ?? 0,
                        Grade = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault()?.Grade ?? 0,
                        Status = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault()?.Status ?? "Pending",
                        UploadDate = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault()?.UploadDate ?? DateTime.MinValue,
                        Notes = s.Submissions.OrderByDescending(sub => sub.UploadDate).FirstOrDefault()?.Notes
                    })
                    .OrderBy(r => r.StudentFullName)
                    .ToListAsync();

                return View(studentSubmissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submissions for tutor.");
                return StatusCode(500, "Failed to load submissions.");
            }
        }

        /// <summary>
        /// Saves tutor feedback or grade for a student's submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFeedback(int submissionId, string feedback, decimal? grade)
        {
            try
            {
                var submission = await _context.Submissions.FindAsync(submissionId);
                if (submission == null) return NotFound("Submission not found.");

                var tutorUserId = _userManager.GetUserId(User);
                if (!await _context.Subjects.AnyAsync(s => s.SubjectId == submission.SubjectId && s.TutorUserId == tutorUserId))
                    return Forbid("You are not authorized to grade this submission.");

                submission.Notes = feedback ?? submission.Notes;
                if (grade.HasValue) submission.Grade = grade.Value;
                submission.Status = string.IsNullOrWhiteSpace(submission.Notes) ? submission.Status : "Graded";

                await _context.SaveChangesAsync();
                TempData["Message"] = "Feedback saved successfully.";

                return RedirectToAction(nameof(Submissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving feedback for submission {SubmissionId}", submissionId);
                return StatusCode(500, "An error occurred while saving feedback.");
            }
        }

        /// <summary>
        /// Loads a submission for editing (feedback or marks).
        /// </summary>
        public async Task<IActionResult> EditSubmission(int id)
        {
            try
            {
                var submission = await _context.Submissions
                    .Include(s => s.Files)
                    .Include(s => s.StudentUser).ThenInclude(su => su.User)
                    .Include(s => s.Subject)
                    .FirstOrDefaultAsync(s => s.SubmissionId == id);

                if (submission == null) return NotFound("Submission not found.");

                var tutorUserId = _userManager.GetUserId(User);
                if (!await _context.Subjects.AnyAsync(s => s.SubjectId == submission.SubjectId && s.TutorUserId == tutorUserId))
                    return Forbid("You are not authorized to edit this submission.");

                return View(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading submission {SubmissionId} for editing.", id);
                return StatusCode(500, "Unable to load submission details.");
            }
        }

        /// <summary>
        /// Updates marks for a submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMarks(int submissionId, decimal grade)
        {
            try
            {
                var submission = await _context.Submissions.FindAsync(submissionId);
                if (submission == null) return NotFound();

                submission.Grade = grade;
                submission.Status = "Complete";
                await _context.SaveChangesAsync();

                TempData["Message"] = "Marks updated successfully.";
                return RedirectToAction(nameof(Submissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marks for submission {SubmissionId}", submissionId);
                return StatusCode(500, "An error occurred while updating marks.");
            }
        }

        /// <summary>
        /// Updates textual feedback for a submission.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeedback(int submissionId, string notes)
        {
            try
            {
                var submission = await _context.Submissions.FindAsync(submissionId);
                if (submission == null) return NotFound();

                submission.Notes = notes;
                await _context.SaveChangesAsync();

                TempData["Message"] = "Feedback saved successfully.";
                return RedirectToAction(nameof(Submissions));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feedback for submission {SubmissionId}", submissionId);
                return StatusCode(500, "Failed to update feedback.");
            }
        }

        /// <summary>
        /// Displays the tutor's planner page.
        /// </summary>
        public IActionResult Planner()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading planner view.");
                return StatusCode(500, "Unable to load planner.");
            }
        }
    }
}
