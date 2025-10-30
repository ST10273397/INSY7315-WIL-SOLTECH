using System.Diagnostics;
using ElevatedTutors.Data;
using ElevatedTutors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ElevatedTutors.Controllers
{
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

        public async Task<IActionResult> Index()
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
                        // If user exists but is not yet approved, send them to pending approval view
                        return RedirectToAction("PendingApproval", "Home");
                    }

                    if (role == "Admin")
                        return RedirectToAction("Dashboard", "Admin");
                    else if (role == "Tutor")
                        return RedirectToAction("Dashboard", "Tutor");
                    else if (role == "Student")
                        return RedirectToAction("Dashboard", "Student");
                }
            }

            // Default for unauthenticated users
            return View();
        }

        public IActionResult PendingApproval()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendContact(string Name, string Email, string Phone, string Message)
        {
            // For now, just show a success message (in production, send an email or save to DB)
            TempData["ContactSuccess"] = $"Thank you {Name}, your message has been received!";
            return RedirectToAction("ContactUs");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
