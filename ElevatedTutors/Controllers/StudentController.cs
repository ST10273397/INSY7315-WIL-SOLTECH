using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevatedTutors.Controllers
{
    public class StudentController : Controller
    {
        [Authorize (Roles ="Student")]
        [Authorize (Policy ="ApprovedOnly")]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Classes()
        {
            return View();
        }

        public IActionResult Schedule()
        {
            return View();
        }

        public IActionResult Report()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }
    }
}
