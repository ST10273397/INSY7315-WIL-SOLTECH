using System.Diagnostics;
using ElevatedTutors.Data;
using ElevatedTutors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ElevatedTutors.Controllers
{
    /// <summary>
    /// Handles public-facing pages (Home, Contact) and user entry routing
    /// based on their role and approval status.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// Default landing page. Redirects signed-in users to their appropriate dashboards
        /// based on roles and approval status.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                if (_signInManager.IsSignedIn(User))
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var role = roles.FirstOrDefault();

                        if (!user.IsApproved)
                        {
                            // Prevent unauthorized access to dashboards if not yet approved
                            _logger.LogInformation("User {UserId} attempted access before approval.", user.Id);
                            return RedirectToAction(nameof(PendingApproval));
                        }

                        // Route to the correct dashboard
                        return role switch
                        {
                            "Admin" => RedirectToAction("Dashboard", "Admin"),
                            "Tutor" => RedirectToAction("Dashboard", "Tutor"),
                            "Student" => RedirectToAction("Dashboard", "Student"),
                            _ => RedirectToAction(nameof(Index))
                        };
                    }
                }

                // Default for guests / unauthenticated users
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing Index() for HomeController.");
                TempData["Error"] = "Something went wrong loading the homepage.";
                return View();
            }
        }

        /// <summary>
        /// View for users waiting for admin approval after registration.
        /// </summary>
        public IActionResult PendingApproval()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering PendingApproval view.");
                TempData["Error"] = "Unable to load pending approval page.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Displays the Contact Us page.
        /// </summary>
        public IActionResult ContactUs()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering ContactUs view.");
                TempData["Error"] = "Unable to load contact page.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Handles the submission of the Contact Us form.
        /// Includes input validation, CSRF protection, and safe feedback messaging.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendContact(string Name, string Email, string Phone, string Message)
        {
            try
            {
                // Basic input validation (in production, use a view model + [Required] attributes)
                if (string.IsNullOrWhiteSpace(Name) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Message))
                {
                    TempData["Error"] = "Please fill in all required fields.";
                    return RedirectToAction(nameof(ContactUs));
                }

                // Sanitize inputs (prevent script injection)
                Name = System.Net.WebUtility.HtmlEncode(Name);
                Email = System.Net.WebUtility.HtmlEncode(Email);
                Phone = System.Net.WebUtility.HtmlEncode(Phone);
                Message = System.Net.WebUtility.HtmlEncode(Message);

                // In production: send an email or save to database
                _logger.LogInformation("Contact form received from {Name} ({Email})", Name, Email);

                TempData["ContactSuccess"] = $"Thank you {Name}, your message has been received!";
                return RedirectToAction(nameof(ContactUs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending contact form submission.");
                TempData["Error"] = "Failed to send your message. Please try again later.";
                return RedirectToAction(nameof(ContactUs));
            }
        }

        /// <summary>
        /// Error page for unexpected exceptions or request tracing.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                var model = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical failure while rendering error page.");
                // Fall back to a minimal inline message
                return Content("An unexpected error occurred. Please refresh or contact support.");
            }
        }
    }
}
