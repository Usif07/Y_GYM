using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;
using Y_GYM.ViewModels;

namespace Y_GYM.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }


        // =========================================================
        // LOGIN
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }


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
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

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
        public IActionResult Register(int? planId = null)
        {
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
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // -----------------------------------------------------
            // Check Username
            // -----------------------------------------------------

            var existingUser =
                await _userManager.FindByNameAsync(model.UserName);


            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "UserName",
                    "This username is already registered.");

                return View(model);
            }


            // -----------------------------------------------------
            // Check Email
            // -----------------------------------------------------

            var existingEmail =
                await _userManager.FindByEmailAsync(model.Email);


            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "This email is already registered.");

                return View(model);
            }


            // -----------------------------------------------------
            // Create Identity User
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
            // Add Member Role
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


            /*
             * IMPORTANT:
             *
             * We do NOT force the user to choose a membership plan.
             *
             * The Identity account is created successfully even when
             * MembershipPlanId is null.
             *
             * The Member business profile will be completed later
             * when the user chooses a membership plan.
             */


            TempData["Success"] =
                "Your account has been created successfully. Please login.";


            return RedirectToAction(nameof(Login));
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                nameof(Login));
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
        // REDIRECT USER BY ROLE
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
                    "Index",
                    "Coach");
            }


            if (await _userManager.IsInRoleAsync(user, "Member"))
            {
                return RedirectToAction(
                    "Index",
                    "Member");
            }


            await _signInManager.SignOutAsync();


            TempData["Error"] =
                "Your account does not have a valid role.";


            return RedirectToAction(
                nameof(Login));
        }
    }
}
