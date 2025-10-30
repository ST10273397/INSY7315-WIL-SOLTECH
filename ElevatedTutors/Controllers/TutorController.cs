using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevatedTutors.Controllers
{
    public class TutorController : Controller
    {
        [Authorize (Roles ="Tutor")]
        [Authorize (Policy ="ApprovedOnly")]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Sessions()
        {
            return View();
        
        }

        public IActionResult Calendar()
        {
            return View();
        }

        public IActionResult Submissions()
        {
            return View();
        }

        public IActionResult Planner()
        {
            return View();
        }
            
    }
}
