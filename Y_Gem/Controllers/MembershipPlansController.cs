using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    public class MembershipPlansController : Controller
    {
        private readonly IMembershipPlans _memPlanRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembershipPlansController(
            IMembershipPlans memPlanRepo,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _memPlanRepo = memPlanRepo;
            _context = context;
            _userManager = userManager;
        }

        // =========================================
        // Membership Plans - Public
        // =========================================

        [AllowAnonymous]
        public IActionResult Index()
        {
            var membershipPlans = _memPlanRepo.GetAll();

            return View(membershipPlans);
        }


        // =========================================
        // Choose Membership Plan
        // =========================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Choose(int planId)
        {
            var plan = _memPlanRepo.GetById(planId);

            if (plan == null)
                return NotFound();

            // User is NOT logged in
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                var returnUrl = Url.Action(
                    nameof(Choose),
                    "MembershipPlans",
                    new { planId });

                return RedirectToAction(
                    "Login",
                    "Account",
                    new { returnUrl });
            }

            // User is logged in
            return RedirectToAction(
                nameof(Checkout),
                new { planId });
        }


        // =========================================
        // Checkout - GET
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Checkout(int planId)
        {
            var plan = _memPlanRepo.GetById(planId);

            if (plan == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            // Find Member profile
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            // Create Member profile if it doesn't exist
            if (member == null)
            {
                member = new Member
                {
                    FullName =
                        user.FullName ??
                        user.UserName ??
                        "Member",

                    Phone =
                        user.PhoneNumber ?? "",

                    JoinDate = DateTime.Now,

                    UserId = userId,

                    MembershipPlanId = null
                };

                _context.Members.Add(member);

                await _context.SaveChangesAsync();
            }

            ViewBag.Plan = plan;
            ViewBag.Member = member;

            return View();
        }


        // =========================================
        // Checkout - POST
        // =========================================

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmCheckout(int planId)
        {
            var plan = _memPlanRepo.GetById(planId);

            if (plan == null)
                return NotFound();

            // =========================================
            // Validate Visit Limit
            // =========================================

            if (plan.TotalVisits <= 0)
            {
                TempData["Error"] =
                    "This membership plan does not have a valid visit limit.";

                return RedirectToAction(nameof(Index));
            }

            // =========================================
            // Get Current User
            // =========================================

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // =========================================
            // Find Member
            // =========================================

            var member = await _context.Members
                .FirstOrDefaultAsync(m =>
                    m.UserId == userId);

            // Create Member if it doesn't exist
            if (member == null)
            {
                var user =
                    await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return NotFound();

                member = new Member
                {
                    FullName =
                        user.FullName ??
                        user.UserName ??
                        "Member",

                    Phone =
                        user.PhoneNumber ?? "",

                    JoinDate = DateTime.Now,

                    UserId = userId,

                    MembershipPlanId = null
                };

                _context.Members.Add(member);

                await _context.SaveChangesAsync();
            }

            // =========================================
            // Create Subscription
            // =========================================

            var startDate =
                DateTime.Today;

            var endDate =
                startDate.AddDays(plan.DurationDays);

            var subscription =
                new Subscription
                {
                    MemberId = member.Id,

                    PlanId = plan.Id,

                    StartDate = startDate,

                    EndDate = endDate,

                    Status = "Active",

                    // Save the visit limit
                    // inside the subscription.
                    TotalVisits = plan.TotalVisits,

                    // New membership starts
                    // with all visits available.
                    RemainingVisits = plan.TotalVisits
                };

            _context.Subscriptions.Add(subscription);

            await _context.SaveChangesAsync();


            // =========================================
            // Create Payment
            // =========================================

            var payment =
                new Payment
                {
                    MemberId = member.Id,

                    SubscriptionId =
                        subscription.Id,

                    Amount =
                        (decimal)plan.Price,

                    PaymentDate =
                        DateTime.Now,

                    StaffId = null
                };

            _context.Payments.Add(payment);


            // =========================================
            // Update Member Membership Plan
            // =========================================

            member.MembershipPlanId =
                plan.Id;

            await _context.SaveChangesAsync();


            // =========================================
            // Success Message
            // =========================================

            TempData["Success"] =
                $"Your {plan.Name} membership has been activated successfully. " +
                $"You have {subscription.RemainingVisits} visits available.";


            // =========================================
            // Go to Member Dashboard
            // =========================================

            return RedirectToAction(
                "Index",
                "Member");
        }


        // =========================================
        // Delete - Admin
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }


        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan != null)
            {
                _memPlanRepo.delete(id);
                _memPlanRepo.save();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // Update - Admin
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(int id)
        {
            var plan = _memPlanRepo.GetById(id);

            if (plan == null)
                return NotFound();

            return View(plan);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Update(MembershipPlan mp)
        {
            if (!ModelState.IsValid)
                return View(mp);

            _memPlanRepo.update(mp);
            _memPlanRepo.save();

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // Create New Plan - Admin
        // =========================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult New()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult SaveNew(MembershipPlan mp)
        {
            if (!ModelState.IsValid)
                return View("New", mp);

            _memPlanRepo.insert(mp);
            _memPlanRepo.save();

            return RedirectToAction(nameof(Index));
        }
    }
}