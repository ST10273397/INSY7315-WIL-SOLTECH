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
        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
            var updateRes = await _userManager.UpdateAsync(user);
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
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

            if (!string.IsNullOrEmpty(role))
                await _userManager.AddToRoleAsync(user, role);

            user.IsApproved = true;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = "User role updated successfully.";
            return RedirectToAction(nameof(AccountPermissions));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAUser(string id, string confirmText)
        {
            if (confirmText?.ToUpper() != "DELETE")
                return RedirectToAction(nameof(PendingUsers));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to delete user.");
            }

            return RedirectToAction(nameof(AccountPermissions));
        }



    }


}
