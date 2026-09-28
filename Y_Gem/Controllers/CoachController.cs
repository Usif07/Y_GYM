using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class CoachController : Controller
    {
        private readonly ICoachRepository _coachRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public CoachController(
            ICoachRepository coachRepo,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _coachRepo = coachRepo;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        // =========================================================
        // PUBLIC - TRAINERS
        // =========================================================

        // GET: Coach
        // Available for Guest, Member, Staff and Admin
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var coaches = await _context.Coaches
                .Include(c => c.User)
                .OrderBy(c => c.User.FullName)
                .ToListAsync();

            return View(coaches);
        }


        // =========================================================
        // PUBLIC - TRAINER DETAILS
        // =========================================================

        // GET: Coach/Details/5
        // Available for Guest, Member, Staff and Admin
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }


        // =========================================================
        // ADMIN - MANAGE TRAINERS
        // =========================================================

        // GET: Coach/Manage
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var coaches = await _context.Coaches
                .Include(c => c.User)
                .OrderBy(c => c.User.FullName)
                .ToListAsync();

            return View(coaches);
        }


        // =========================================================
        // ADMIN - CREATE TRAINER
        // =========================================================

        // GET: Coach/Create
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // POST: Coach/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string fullName,
            string userName,
            string email,
            string password,
            string phone,
            string coachSpecialty)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            password = password?.Trim() ?? string.Empty;
            phone = phone?.Trim() ?? string.Empty;
            coachSpecialty = coachSpecialty?.Trim() ?? string.Empty;

            // -----------------------------------------------------
            // Basic Validation
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(coachSpecialty))
            {
                ModelState.AddModelError(
                    "",
                    "Please fill in all required fields.");

                return View();
            }


            // -----------------------------------------------------
            // Check Username
            // -----------------------------------------------------

            var existingUsername =
                await _userManager.FindByNameAsync(userName);

            if (existingUsername != null)
            {
                ModelState.AddModelError(
                    "",
                    "This username is already in use.");

                return View();
            }


            // -----------------------------------------------------
            // Check Email
            // -----------------------------------------------------

            var existingEmail =
                await _userManager.FindByEmailAsync(email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "",
                    "This email is already in use.");

                return View();
            }


            // -----------------------------------------------------
            // Create Application User
            // -----------------------------------------------------

            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
                PhoneNumber = phone,
                FullName = fullName,
                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }


            // -----------------------------------------------------
            // Make Sure Trainer Role Exists
            // -----------------------------------------------------

            if (!await _roleManager.RoleExistsAsync("Trainer"))
            {
                var roleResult =
                    await _roleManager.CreateAsync(
                        new IdentityRole("Trainer"));

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View();
                }
            }


            // -----------------------------------------------------
            // Add User To Trainer Role
            // -----------------------------------------------------

            var roleAddResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Trainer");

            if (!roleAddResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleAddResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }


            // -----------------------------------------------------
            // Create Coach Profile
            // -----------------------------------------------------

            var coach = new Coach
            {
                UserId = user.Id,
                CoachSpecialty = coachSpecialty
            };

            _coachRepo.Add(coach);
            _coachRepo.Save();


            TempData["SuccessMessage"] =
                "Trainer account created successfully.";

            return RedirectToAction(nameof(Manage));
        }


        // =========================================================
        // ADMIN - EDIT TRAINER
        // =========================================================

        // GET: Coach/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }


        // POST: Coach/Edit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string fullName,
            string email,
            string phone,
            string coachSpecialty)
        {
            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }


            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            phone = phone?.Trim() ?? string.Empty;
            coachSpecialty = coachSpecialty?.Trim() ?? string.Empty;


            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(coachSpecialty))
            {
                ModelState.AddModelError(
                    "",
                    "Please fill in all required fields.");

                return View(coach);
            }


            // -----------------------------------------------------
            // Check Email
            // -----------------------------------------------------

            var existingEmail =
                await _userManager.FindByEmailAsync(email);

            if (existingEmail != null &&
                existingEmail.Id != coach.UserId)
            {
                ModelState.AddModelError(
                    "",
                    "This email is already in use.");

                return View(coach);
            }


            // -----------------------------------------------------
            // Update Application User
            // -----------------------------------------------------

            coach.User.FullName = fullName;
            coach.User.Email = email;
            coach.User.PhoneNumber = phone;


            // -----------------------------------------------------
            // Update Coach Profile
            // -----------------------------------------------------

            coach.CoachSpecialty = coachSpecialty;


            _context.Coaches.Update(coach);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Trainer information updated successfully.";

            return RedirectToAction(nameof(Manage));
        }


        // =========================================================
        // ADMIN - DELETE TRAINER
        // =========================================================

        // GET: Coach/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }


        // POST: Coach/Delete
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coach = await _context.Coaches
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (coach == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Delete Coach Profile
            // -----------------------------------------------------

            _context.Coaches.Remove(coach);

            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // Delete Identity Account
            // -----------------------------------------------------

            if (coach.User != null)
            {
                var deleteUserResult =
                    await _userManager.DeleteAsync(coach.User);

                if (!deleteUserResult.Succeeded)
                {
                    foreach (var error in deleteUserResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("Delete", coach);
                }
            }


            TempData["SuccessMessage"] =
                "Trainer deleted successfully.";

            return RedirectToAction(nameof(Manage));
        }
    }
}