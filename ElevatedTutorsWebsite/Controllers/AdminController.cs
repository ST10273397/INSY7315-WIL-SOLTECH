using ElevatedTutorsWebsite.Models;
using ElevatedTutorsWebsite.Data;
using Microsoft.AspNetCore.Mvc;

namespace ElevatedTutorsWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Add the main User
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // This generates the UserId

                // 2. Create the role-specific entity based on Role
                switch (user.Role)
                {
                    case "Student":
                        var studentUser = new StudentUser
                        {
                            UserId = user.UserId,
                            ParentEmail = Request.Form["ParentEmail"],
                            MaxSessions = int.Parse(Request.Form["MaxSessions"])
                        };
                        _context.StudentUsers.Add(studentUser);
                        break;

                    case "Tutor":
                        var tutorUser = new TutorUser
                        {
                            UserId = user.UserId
                        };
                        _context.TutorUsers.Add(tutorUser);
                        break;

                    case "Admin":
                        var adminUser = new AdminUser
                        {
                            UserId = user.UserId
                        };
                        _context.AdminUsers.Add(adminUser);
                        break;

                    default:
                        throw new InvalidOperationException("Invalid user role.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "User has been created successfully!";
                return RedirectToAction("Index");
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while creating the user.";
                return View(user);
            }
        }

    }
}
