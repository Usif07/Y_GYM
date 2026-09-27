using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.ViewModels;

namespace Y_GYM.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        // =========================================================
        // LOGIN - GET
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        // =========================================================
        // LOGIN - POST
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string username,
            string password,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Username and password are required.");

                return View();
            }

            var user =
                await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid username or password.");

                return View();
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    user,
                    password,
                    isPersistent: false,
                    lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // -------------------------------------------------
                // If user came from another page, return there.
                // Example:
                // /MembershipPlans/Choose?planId=2
                // -------------------------------------------------

                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // -------------------------------------------------
                // Otherwise redirect according to role.
                // -------------------------------------------------

                return await RedirectByRole(user);
            }

            ModelState.AddModelError(
                "",
                "Invalid username or password.");

            return View();
        }

        // =========================================================
        // REGISTER - GET
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(
            int? planId = null,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var model = new RegisterViewModel
            {
                MembershipPlanId = planId
            };

            return View(model);
        }

        // =========================================================
        // REGISTER - POST
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // -----------------------------------------------------
            // Check username
            // -----------------------------------------------------

            var existingUser =
                await _userManager.FindByNameAsync(
                    model.UserName);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "This username is already registered.");

                return View(model);
            }

            // -----------------------------------------------------
            // Check email
            // -----------------------------------------------------

            var existingEmail =
                await _userManager.FindByEmailAsync(
                    model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already registered.");

                return View(model);
            }

            // -----------------------------------------------------
            // Check phone
            // -----------------------------------------------------

            var existingMember =
                await _context.Members
                    .FirstOrDefaultAsync(
                        m => m.Phone == model.Phone);

            if (existingMember != null)
            {
                ModelState.AddModelError(
                    "Phone",
                    "This phone number is already registered.");

                return View(model);
            }

            // -----------------------------------------------------
            // Create ApplicationUser
            // -----------------------------------------------------

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.Phone
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            // -----------------------------------------------------
            // Add Member role
            // -----------------------------------------------------

            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Member");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View(model);
            }

            // -----------------------------------------------------
            // Create Member profile
            //
            // IMPORTANT:
            // The account can exist without a membership.
            // MembershipPlanId stays NULL until the member
            // actually pays for a plan.
            // -----------------------------------------------------

            var member = new Member
            {
                UserId = user.Id,

                FullName = model.FullName,

                Phone = model.Phone,

                JoinDate = DateTime.Now,

                MembershipPlanId = null
            };

            _context.Members.Add(member);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // If Member creation fails, remove the Identity user
                // so we don't leave an incomplete account behind.

                await _userManager.RemoveFromRoleAsync(
                    user,
                    "Member");

                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    "",
                    "The account could not be completed. Please try again.");

                return View(model);
            }

            // -----------------------------------------------------
            // Account created successfully
            // -----------------------------------------------------

            TempData["Success"] =
                "Your account has been created successfully. Please login.";

            // -----------------------------------------------------
            // Keep the original returnUrl.
            //
            // Example:
            // /MembershipPlans/Choose?planId=2
            //
            // So after login the user can continue with the
            // membership plan he selected.
            // -----------------------------------------------------

            return RedirectToAction(
                nameof(Login),
                new
                {
                    returnUrl
                });
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        // =========================================================
        // ACCESS DENIED
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // =========================================================
        // REDIRECT BY ROLE
        // =========================================================

        private async Task<IActionResult> RedirectByRole(
            ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Admin");
            }

            if (await _userManager.IsInRoleAsync(user, "Staff"))
            {
                return RedirectToAction(
                    "Index",
                    "Staff");
            }

            if (await _userManager.IsInRoleAsync(user, "Trainer"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Trainer");
            }

            if (await _userManager.IsInRoleAsync(user, "Member"))
            {
                return RedirectToAction(
                    "Index",
                    "Member");
            }

            // -----------------------------------------------------
            // User has no valid role
            // -----------------------------------------------------

            await _signInManager.SignOutAsync();

            TempData["Error"] =
                "Your account does not have a valid role.";

            return RedirectToAction(nameof(Login));
        }
    }
}