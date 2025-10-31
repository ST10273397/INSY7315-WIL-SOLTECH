using ElevatedTutors.Data;
using ElevatedTutors.Models;
using ElevatedTutors.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElevatedTutors.Controllers
{
    /// <summary>
    /// Admin controller handles user approval, management, dashboard stats, and role assignments.
    /// Only accessible to users with the "Admin" role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Displays all users who have not yet been approved.
        /// </summary>
        public async Task<IActionResult> PendingUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => !u.IsApproved)
                    .ToListAsync();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    user.Roles = roles.ToList();
                }

                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching pending users.");
                TempData["Error"] = "An error occurred while retrieving pending users.";
                return View(new List<ApplicationUser>());
            }
        }

        /// <summary>
        /// Approves a user and assigns them a role.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string id, string role)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Invalid request parameters.";
                return RedirectToAction(nameof(PendingUsers));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(PendingUsers));
            }

            user.IsApproved = true;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!await _context.Roles.AnyAsync(r => r.Name == role))
                {
                    TempData["Error"] = $"Role '{role}' does not exist.";
                    return RedirectToAction(nameof(PendingUsers));
                }

                await _userManager.UpdateAsync(user);

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);

                    switch (role)
                    {
                        case "Student":
                            _context.StudentUsers.Add(new StudentUser { UserId = user.Id });
                            break;
                        case "Tutor":
                            _context.TutorUsers.Add(new TutorUser { UserId = user.Id });
                            break;
                        case "Admin":
                            _context.AdminUsers.Add(new AdminUser { UserId = user.Id });
                            break;
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "User approved successfully!";
                return RedirectToAction(nameof(PendingUsers));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error approving user with ID {UserId}", id);
                TempData["Error"] = "An unexpected error occurred while approving the user.";
                return RedirectToAction(nameof(PendingUsers));
            }
        }

        /// <summary>
        /// Displays the admin dashboard with counts of tutors, students, and pending users.
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var allUsers = await _userManager.Users.ToListAsync();
                var tutors = new List<ApplicationUser>();
                var students = new List<ApplicationUser>();

                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Tutor")) tutors.Add(user);
                    else if (roles.Contains("Student")) students.Add(user);
                }

                var model = new AdminDashboardViewModel
                {
                    TotalMembers = tutors.Count + students.Count,
                    TutorCount = tutors.Count,
                    StudentCount = students.Count,
                    PendingUsersCount = allUsers.Count(u => !u.IsApproved)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard.");
                TempData["Error"] = "Failed to load dashboard data.";
                return View(new AdminDashboardViewModel());
            }
        }

        /// <summary>
        /// API endpoint for fetching dashboard stats dynamically (AJAX).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var tutorCount = 0;
                var studentCount = 0;

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Tutor")) tutorCount++;
                    if (roles.Contains("Student")) studentCount++;
                }

                var pendingCount = users.Count(u => !u.IsApproved);
                var totalMembers = tutorCount + studentCount;

                return Json(new
                {
                    totalMembers,
                    tutorCount,
                    studentCount,
                    pendingCount,
                    updatedAt = DateTime.Now.ToString("HH:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard stats.");
                return Json(new { error = "Failed to load dashboard stats." });
            }
        }

        /// <summary>
        /// Displays a list of all users and their roles for permission management.
        /// </summary>
        public async Task<IActionResult> AccountPermissions()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = new List<(ApplicationUser User, IList<string> Roles)>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userList.Add((user, roles));
                }

                return View(userList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account permissions.");
                TempData["Error"] = "An error occurred loading account permissions.";
                return View(new List<(ApplicationUser, IList<string>)>());
            }
        }

        public IActionResult Notifications() => View();
        public IActionResult Payroll() => View();

        /// <summary>
        /// Deletes a user after confirmation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id, string confirmText)
        {
            if (confirmText?.ToUpper() != "DELETE")
            {
                TempData["Error"] = "Type DELETE in the confirmation box.";
                return RedirectToAction(nameof(AccountPermissions));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(AccountPermissions));
                }

                var roles = await _userManager.GetRolesAsync(user);
                foreach (var r in roles)
                    await _userManager.RemoveFromRoleAsync(user, r);

                // Remove related entities
                _context.StudentUsers.RemoveRange(_context.StudentUsers.Where(s => s.UserId == id));
                _context.TutorUsers.RemoveRange(_context.TutorUsers.Where(t => t.UserId == id));
                _context.AdminUsers.RemoveRange(_context.AdminUsers.Where(a => a.UserId == id));

                await _context.SaveChangesAsync();
                await _userManager.DeleteAsync(user);

                TempData["Success"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
                TempData["Error"] = "Failed to delete user. Please try again.";
            }

            return RedirectToAction(nameof(AccountPermissions));
        }

        /// <summary>
        /// Edits a user's role, ensuring only valid role changes.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(string id, string role)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid user ID.";
                return RedirectToAction(nameof(AccountPermissions));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(AccountPermissions));
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                user.IsApproved = true;

                // Remove all old role entities
                _context.StudentUsers.RemoveRange(_context.StudentUsers.Where(s => s.UserId == user.Id));
                _context.TutorUsers.RemoveRange(_context.TutorUsers.Where(t => t.UserId == user.Id));
                _context.AdminUsers.RemoveRange(_context.AdminUsers.Where(a => a.UserId == user.Id));

                // Assign new role
                if (!string.IsNullOrEmpty(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                    switch (role)
                    {
                        case "Student": _context.StudentUsers.Add(new StudentUser { UserId = user.Id }); break;
                        case "Tutor": _context.TutorUsers.Add(new TutorUser { UserId = user.Id }); break;
                        case "Admin": _context.AdminUsers.Add(new AdminUser { UserId = user.Id }); break;
                    }
                }

                await _context.SaveChangesAsync();
                await _userManager.UpdateAsync(user);

                TempData["Success"] = "User role updated successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role for user ID {UserId}", id);
                TempData["Error"] = "Failed to update user role.";
            }

            return RedirectToAction(nameof(AccountPermissions));
        }
    }
}
