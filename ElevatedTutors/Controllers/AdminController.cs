using ElevatedTutors.Data;
using ElevatedTutors.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ElevatedTutors.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _context = dbContext;
        }
        public async Task<IActionResult> PendingUsers()
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


        public async Task<IActionResult> Approve(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

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
                        case "Student": _context.StudentUsers.Add(new StudentUser { UserId = user.Id }); break;
                        case "Tutor": _context.TutorUsers.Add(new TutorUser { UserId = user.Id }); break;
                        case "Admin": _context.AdminUsers.Add(new AdminUser { UserId = user.Id }); break;
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "User has been approved successfully!";
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while approving the user.";
            }

            return RedirectToAction(nameof(PendingUsers));
        }


        public async Task<IActionResult> Dashboard()
        {
            // Get all users except Admins
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

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            var users = _userManager.Users.ToList();
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


        
        public async Task<IActionResult> AccountPermissions()
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


        public IActionResult Notifications()
        {
            return View();
        }

        public IActionResult Payroll()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id, string confirmText)
        {
            if (confirmText?.ToUpper() != "DELETE")
            {
                TempData["Message"] = "Type DELETE in the confirmation box.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // remove roles / logins then delete
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var r in roles)
            {
                await _userManager.RemoveFromRoleAsync(user, r);
            }

            var student = await _context.StudentUsers.FirstOrDefaultAsync(s => s.UserId == id);
            if (student != null) _context.StudentUsers.Remove(student);

            var tutor = await _context.TutorUsers.FirstOrDefaultAsync(t => t.UserId == id);
            if (tutor != null) _context.TutorUsers.Remove(tutor);

            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.UserId == id);
            if (admin != null) _context.AdminUsers.Remove(admin);

            await _context.SaveChangesAsync();


            await _userManager.DeleteAsync(user);

            TempData["Message"] = "User deleted.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            user.IsApproved = true;

            // Remove existing role entities
            var student = await _context.StudentUsers.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (student != null) _context.StudentUsers.Remove(student);

            var tutor = await _context.TutorUsers.FirstOrDefaultAsync(t => t.UserId == user.Id);
            if (tutor != null) _context.TutorUsers.Remove(tutor);

            var admin = await _context.AdminUsers.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (admin != null) _context.AdminUsers.Remove(admin);

            // Now assign the new role
            if (!string.IsNullOrEmpty(role))
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
            }

            await _context.SaveChangesAsync();
            await _userManager.UpdateAsync(user);


            TempData["SuccessMessage"] = "User role updated successfully.";
            return RedirectToAction(nameof(AccountPermissions));
        }
    }
}
