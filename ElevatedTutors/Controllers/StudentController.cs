using ElevatedTutors.Data;
using ElevatedTutors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElevatedTutors.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace ElevatedTutors.Controllers
{
    [Authorize(Roles = "Student")] // ✅ Restrict access to authenticated students only
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger; // ✅ Add logging

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<StudentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Displays the student dashboard with classes, due dates, and announcements.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var studentUserId = _userManager.GetUserId(User);
                if (studentUserId == null)
                    return Unauthorized("User is not logged in.");

                var student = await _context.StudentUsers
                    .Include(s => s.Subjects)
                        .ThenInclude(sub => sub.TutorUser)
                            .ThenInclude(t => t.User)
                    .FirstOrDefaultAsync(s => s.UserId == studentUserId);

                if (student == null)
                {
                    _logger.LogWarning("Student not found for User ID: {UserId}", studentUserId);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Student Dashboard.");
                return StatusCode(500, "An unexpected error occurred while loading the dashboard.");
            }
        }

        /// <summary>
        /// Displays all classes the student is enrolled in.
        /// </summary>
        public async Task<IActionResult> Classes()
        {
            try
            {
                var studentUserId = _userManager.GetUserId(User);
                if (studentUserId == null)
                    return Unauthorized();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student classes.");
                return StatusCode(500, "An error occurred while retrieving classes.");
            }
        }

        /// <summary>
        /// Displays the student's schedule including sessions and due dates.
        /// </summary>
        public async Task<IActionResult> Schedule()
        {
            try
            {
                var studentUserId = _userManager.GetUserId(User);
                if (studentUserId == null)
                    return Unauthorized();

                var sessions = await _context.Sessions
                    .Include(s => s.Subject)
                    .Include(s => s.TutorUser)
                        .ThenInclude(t => t.User)
                    .Where(s => s.StudentUserId == studentUserId)
                    .ToListAsync();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student schedule.");
                return StatusCode(500, "Unable to load schedule at this time.");
            }
        }

        /// <summary>
        /// Displays all past submissions and grades for the student.
        /// </summary>
        public async Task<IActionResult> Report()
        {
            try
            {
                var studentUserId = _userManager.GetUserId(User);
                if (studentUserId == null)
                    return Unauthorized();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student report.");
                return StatusCode(500, "Failed to load report data.");
            }
        }

        /// <summary>
        /// Displays feedback for a specific submission.
        /// </summary>
        public async Task<IActionResult> Feedback(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("Invalid submission ID.");

                var submission = await _context.Submissions
                    .Include(s => s.Subject)
                    .FirstOrDefaultAsync(s => s.SubmissionId == id);

                if (submission == null)
                {
                    _logger.LogWarning("Submission not found with ID {Id}", id);
                    return NotFound("Submission not found.");
                }

                // Optional security check: ensure student owns this submission
                var userId = _userManager.GetUserId(User);
                if (submission.UserId != userId)
                    return Forbid("You are not authorized to view this feedback.");

                return View(submission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading feedback for submission {Id}", id);
                return StatusCode(500, "Unable to load feedback at this time.");
            }
        }

        /// <summary>
        /// Contact form page for students to reach support or admin.
        /// </summary>
        [AllowAnonymous] // Optional: if you want even non-logged-in users to access
        public IActionResult ContactUs()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Contact Us page.");
                return StatusCode(500, "Error loading contact page.");
            }
        }
    }
}
