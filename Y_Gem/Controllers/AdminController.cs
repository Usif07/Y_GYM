using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.ViewModels;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var model = new AdminDashboardViewModel
            {
                TotalUsers =
                    await _context.Users.CountAsync(),

                TotalMembers =
                    await _context.Members.CountAsync(),

                TotalStaff =
                    await _context.Staff.CountAsync(),

                TotalCoaches =
                    await _context.Coaches.CountAsync(),

                TotalMembershipPlans =
                    await _context.MembershipPlans.CountAsync(),

                TotalSubscriptions =
                    await _context.Subscriptions.CountAsync(),

                ActiveSubscriptions =
                    await _context.Subscriptions
                        .CountAsync(s => s.EndDate >= now),

                TotalPayments =
                    await _context.Payments.CountAsync(),

                TotalRevenue =
                    await _context.Payments
                        .Select(p => (decimal?)p.Amount)
                        .SumAsync() ?? 0,

                TotalClasses =
                    await _context.Classes.CountAsync(),

                TotalSchedules =
                    await _context.ClassSchedules.CountAsync(),

                TotalBookings =
                    await _context.Bookings.CountAsync(),

                TotalCheckIns =
                    await _context.CheckIns.CountAsync(),

                TotalDietPlans =
                    await _context.DietPlans.CountAsync(),

                TotalProgressRecords =
                    await _context.ProgressLogs.CountAsync()
            };

            return View(model);
        }

        // =========================================================
        // ADMIN USERS
        // =========================================================

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // =========================================================
        // CREATE ADMIN
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser item,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password is required.");

                return View(item);
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            item.EmailConfirmed = true;

            var result =
                await _userManager.CreateAsync(item, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(item);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    item,
                    "Admin");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(item);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(item);
            }

            TempData["Success"] =
                "Administrator account created successfully.";

            return RedirectToAction(nameof(Users));
        }

        // =========================================================
        // EDIT
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            var existingUser =
                await _userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FullName =
                item.FullName;

            existingUser.UserName =
                item.UserName;

            existingUser.Email =
                item.Email;

            existingUser.PhoneNumber =
                item.PhoneNumber;

            var result =
                await _userManager.UpdateAsync(existingUser);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Administrator information updated successfully.";

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(item);
        }

        // =========================================================
        // DELETE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var currentUserId =
                _userManager.GetUserId(User);

            if (currentUserId == id)
            {
                TempData["Error"] =
                    "You cannot delete your own administrator account.";

                return RedirectToAction(nameof(Users));
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return RedirectToAction(nameof(Users));
            }

            var isAdmin =
                await _userManager.IsInRoleAsync(
                    user,
                    "Admin");

            if (!isAdmin)
            {
                TempData["Error"] =
                    "This user is not an administrator.";

                return RedirectToAction(nameof(Users));
            }

            var result =
                await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Administrator account deleted successfully.";

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View("Delete", user);
        }
    }
}
