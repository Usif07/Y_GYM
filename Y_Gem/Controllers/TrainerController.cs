using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TrainerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================================================
        // TRAINER DASHBOARD
        // =========================================================

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }


            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);


            if (coach == null)
            {
                return NotFound(
                    "Trainer profile was not found.");
            }


            var schedules = await _context.ClassSchedules
                .Include(s => s.Class)
                .Where(s => s.CoachId == coach.Id)
                .OrderBy(s => s.StartTime)
                .ToListAsync();


            ViewBag.Coach = coach;
            ViewBag.Schedules = schedules;

            return View();
        }
    }
}