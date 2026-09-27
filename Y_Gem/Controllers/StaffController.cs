using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Models.ViewModels;
using Y_GYM.Repository;

namespace Y_GYM.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : Controller
    {
        private readonly IStaffRepository _staffRepo;
        private readonly IMemberRepository _memberRepo;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

    public StaffController(
        IStaffRepository staffRepo,
        IMemberRepository memberRepo,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
        {
            _staffRepo = staffRepo;
            _memberRepo = memberRepo;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var isAdmin = User.IsInRole("Admin");

            Staff? staff = null;

            if (!isAdmin)
            {
                staff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (staff == null)
                    return NotFound("Staff profile was not found.");
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalMembers =
                await _context.Members.CountAsync();

            var activeMembers =
                await _context.Subscriptions
                    .Where(s =>
                        s.Status == "Active" &&
                        s.StartDate <= today &&
                        s.EndDate >= today)
                    .Select(s => s.MemberId)
                    .Distinct()
                    .CountAsync();

            var todayCheckIns =
                await _context.CheckIns
                    .CountAsync(c =>
                        c.CheckInTime >= today &&
                        c.CheckInTime < tomorrow &&
                        c.Status == "Allowed");

            var todayPayments =
                await _context.Payments
                    .Where(p =>
                        p.PaymentDate >= today &&
                        p.PaymentDate < tomorrow)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var recentCheckIns =
                await _context.CheckIns
                    .Include(c => c.Member)
                        .ThenInclude(m => m.User)
                    .OrderByDescending(c => c.CheckInTime)
                    .Take(8)
                    .ToListAsync();

            var recentPayments =
                await _context.Payments
                    .Include(p => p.Member)
                        .ThenInclude(m => m.User)
                    .Include(p => p.Subscription)
                        .ThenInclude(s => s.MembershipPlan)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(8)
                    .ToListAsync();

            var vm = new StaffDashboardVM
            {
                Staff = staff,
                IsAdmin = isAdmin,
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                TodayCheckIns = todayCheckIns,
                TodayPayments = todayPayments,
                RecentCheckIns = recentCheckIns,
                RecentPayments = recentPayments
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult RegisterMember()
        {
            return View(new RegisterMemberVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterMember(RegisterMemberVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            vm.UserName = vm.UserName.Trim();
            vm.Email = vm.Email.Trim();
            vm.Phone = vm.Phone.Trim();

            var existingUsername =
                await _userManager.FindByNameAsync(vm.UserName);

            if (existingUsername != null)
            {
                ModelState.AddModelError(
                    nameof(vm.UserName),
                    "This username is already registered.");

                return View(vm);
            }

            var existingEmail =
                await _userManager.FindByEmailAsync(vm.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    nameof(vm.Email),
                    "This email is already registered.");

                return View(vm);
            }

            var existingPhone =
                await _context.Members
                    .AnyAsync(m => m.Phone == vm.Phone);

            if (existingPhone)
            {
                ModelState.AddModelError(
                    nameof(vm.Phone),
                    "This phone number is already registered.");

                return View(vm);
            }

            var newUser = new ApplicationUser
            {
                UserName = vm.UserName,
                Email = vm.Email,
                FullName = vm.FullName.Trim(),
                PhoneNumber = vm.Phone,
                EmailConfirmed = true
            };

            var userResult =
                await _userManager.CreateAsync(
                    newUser,
                    vm.Password);

            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(vm);
            }

            var roleResult =
                await _userManager.AddToRoleAsync(
                    newUser,
                    "Member");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(vm);
            }

            var member = new Member
            {
                UserId = newUser.Id,
                FullName = vm.FullName.Trim(),
                Phone = vm.Phone,
                Weight = vm.Weight,
                Height = vm.Height,
                Goal = vm.Goal,
                FitnessLevel = vm.FitnessLevel,
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
                await _userManager.RemoveFromRoleAsync(
                    newUser,
                    "Member");

                await _userManager.DeleteAsync(newUser);

                ModelState.AddModelError(
                    string.Empty,
                    "The member account could not be completed. Please try again.");

                return View(vm);
            }

            TempData["Success"] =
                $"Member {member.FullName} has been registered successfully.";

            TempData["MemberUsername"] =
                newUser.UserName;

            return RedirectToAction(nameof(RegisterMember));
        }

        [HttpGet]
        public async Task<IActionResult> RegisterPayment()
        {
            var plans = await _context.MembershipPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            ViewBag.Plans = plans;

            return View(new RegisterPaymentVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPayment(
            RegisterPaymentVM vm)
        {
            var plans = await _context.MembershipPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            ViewBag.Plans = plans;

            if (!ModelState.IsValid)
                return View(vm);

            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            Staff? staff = null;

            if (!User.IsInRole("Admin"))
            {
                staff = await _context.Staff
                    .FirstOrDefaultAsync(
                        s => s.UserId == userId);

                if (staff == null)
                {
                    TempData["Error"] =
                        "Staff profile was not found.";

                    return RedirectToAction(
                        nameof(RegisterPayment));
                }
            }

            var member =
                await _context.Members
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(
                        m => m.Phone == vm.Phone);

            if (member == null)
            {
                ModelState.AddModelError(
                    nameof(vm.Phone),
                    "No member was found with this phone number.");

                return View(vm);
            }

            var plan =
                await _context.MembershipPlans
                    .FirstOrDefaultAsync(
                        p => p.Id == vm.PlanId);

            if (plan == null)
            {
                ModelState.AddModelError(
                    nameof(vm.PlanId),
                    "The selected membership plan does not exist.");

                return View(vm);
            }

            if (plan.TotalVisits <= 0)
            {
                ModelState.AddModelError(
                    nameof(vm.PlanId),
                    "The selected membership plan has no valid visit limit.");

                return View(vm);
            }

            if (vm.AmountPaid <= 0)
            {
                ModelState.AddModelError(
                    nameof(vm.AmountPaid),
                    "Amount paid must be greater than zero.");

                return View(vm);
            }

            var pendingSubscription =
                await _context.Subscriptions
                    .FirstOrDefaultAsync(s =>
                        s.MemberId == member.Id &&
                        s.PlanId == plan.Id &&
                        s.Status == "Pending");

            if (pendingSubscription != null)
            {
                var previousPayments =
                    await _context.Payments
                        .Where(p =>
                            p.SubscriptionId ==
                            pendingSubscription.Id)
                        .SumAsync(p =>
                            (decimal?)p.Amount) ?? 0;

                var planPrice =
                    (decimal)plan.Price;

                var amountRemainingBeforePayment =
                    planPrice - previousPayments;

                if (amountRemainingBeforePayment <= 0)
                {
                    pendingSubscription.Status =
                        "Active";

                    pendingSubscription.TotalVisits =
                        plan.TotalVisits;

                    pendingSubscription.RemainingVisits =
                        plan.TotalVisits;

                    member.MembershipPlanId =
                        plan.Id;

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        $"The membership for {member.FullName} is now Active.";

                    return RedirectToAction(nameof(Index));
                }

                var newTotalPaid =
                    previousPayments +
                    vm.AmountPaid;

                var remainingAfterPayment =
                    planPrice -
                    newTotalPaid;

                var strategy =
                    _context.Database
                        .CreateExecutionStrategy();

                try
                {
                    await strategy.ExecuteAsync(async () =>
                    {
                        await using var transaction =
                            await _context.Database
                                .BeginTransactionAsync();

                        try
                        {
                            var payment = new Payment
                            {
                                MemberId =
                                    member.Id,

                                SubscriptionId =
                                    pendingSubscription.Id,

                                Amount =
                                    vm.AmountPaid,

                                PaymentDate =
                                    DateTime.Now,

                                StaffId =
                                    staff?.Id
                            };

                            _context.Payments.Add(payment);

                            if (newTotalPaid >= planPrice)
                            {
                                pendingSubscription.Status =
                                    "Active";

                                pendingSubscription.TotalVisits =
                                    plan.TotalVisits;

                                pendingSubscription.RemainingVisits =
                                    plan.TotalVisits;

                                member.MembershipPlanId =
                                    plan.Id;
                            }

                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    });

                    if (newTotalPaid < planPrice)
                    {
                        TempData["Success"] =
                            $"Payment recorded for {member.FullName}. " +
                            $"Subscription is still Pending. " +
                            $"Remaining amount: {remainingAfterPayment:0.00} EGP";

                        return RedirectToAction(nameof(Index));
                    }

                    var change =
                        newTotalPaid - planPrice;

                    TempData["Success"] =
                        $"Payment completed successfully for {member.FullName}. " +
                        $"Membership is now Active. " +
                        $"Change: {change:0.00} EGP";

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["Error"] =
                        "An error occurred while processing the payment.";

                    return RedirectToAction(
                        nameof(RegisterPayment));
                }
            }

            var activeSubscription =
                await _context.Subscriptions
                    .FirstOrDefaultAsync(s =>
                        s.MemberId == member.Id &&
                        s.PlanId == plan.Id &&
                        s.Status == "Active" &&
                        s.StartDate <= DateTime.Today &&
                        s.EndDate >= DateTime.Today);

            if (activeSubscription != null)
            {
                ModelState.AddModelError(
                    nameof(vm.PlanId),
                    "This member already has an active membership for this plan.");

                return View(vm);
            }

            var planPriceNew =
                (decimal)plan.Price;

            var isFullyPaid =
                vm.AmountPaid >= planPriceNew;

            var subscriptionStatus =
                isFullyPaid
                    ? "Active"
                    : "Pending";

            var startDate =
                DateTime.Today;

            var endDate =
                startDate.AddDays(
                    plan.DurationDays);

            var strategyNew =
                _context.Database
                    .CreateExecutionStrategy();

            try
            {
                await strategyNew.ExecuteAsync(async () =>
                {
                    await using var transaction =
                        await _context.Database
                            .BeginTransactionAsync();

                    try
                    {
                        var subscription =
                            new Subscription
                            {
                                MemberId =
                                    member.Id,

                                PlanId =
                                    plan.Id,

                                StartDate =
                                    startDate,

                                EndDate =
                                    endDate,

                                Status =
                                    subscriptionStatus,

                                TotalVisits =
                                    plan.TotalVisits,

                                RemainingVisits =
                                    plan.TotalVisits
                            };

                        _context.Subscriptions.Add(
                            subscription);

                        await _context.SaveChangesAsync();

                        var payment =
                            new Payment
                            {
                                MemberId =
                                    member.Id,

                                SubscriptionId =
                                    subscription.Id,

                                Amount =
                                    vm.AmountPaid,

                                PaymentDate =
                                    DateTime.Now,

                                StaffId =
                                    staff?.Id
                            };

                        _context.Payments.Add(
                            payment);

                        if (isFullyPaid)
                        {
                            member.MembershipPlanId =
                                plan.Id;
                        }

                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                if (!isFullyPaid)
                {
                    var remaining =
                        planPriceNew -
                        vm.AmountPaid;

                    TempData["Success"] =
                        $"Payment recorded for {member.FullName}. " +
                        $"Subscription is Pending. " +
                        $"Remaining amount: {remaining:0.00} EGP";

                    return RedirectToAction(nameof(Index));
                }

                var changeNew =
                    vm.AmountPaid -
                    planPriceNew;

                TempData["Success"] =
                    $"Payment completed successfully for {member.FullName}. " +
                    $"Membership is now Active. " +
                    $"Change: {changeNew:0.00} EGP";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Error"] =
                    "An error occurred while processing the payment.";

                return RedirectToAction(
                    nameof(RegisterPayment));
            }
        }

        [HttpGet]
        public IActionResult CheckIn()
        {
            return View(new CheckInVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(CheckInVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            Staff? staff = null;

            if (!User.IsInRole("Admin"))
            {
                staff = await _context.Staff
                    .FirstOrDefaultAsync(
                        s => s.UserId == userId);

                if (staff == null)
                {
                    TempData["Error"] =
                        "Staff profile was not found.";

                    return RedirectToAction(
                        nameof(CheckIn));
                }
            }

            var member =
                await _context.Members
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(
                        m => m.Phone == vm.Phone);

            if (member == null)
            {
                TempData["Error"] =
                    "No member was found with this phone number.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            var activeSub =
                await _context.Subscriptions
                    .Include(s => s.MembershipPlan)
                    .Where(s =>
                        s.MemberId == member.Id &&
                        s.Status == "Active" &&
                        s.StartDate <= DateTime.Today &&
                        s.EndDate >= DateTime.Today)
                    .OrderByDescending(
                        s => s.EndDate)
                    .FirstOrDefaultAsync();

            if (activeSub == null)
            {
                var rejectedCheckIn =
                    new CheckIn
                    {
                        MemberId =
                            member.Id,

                        StaffId =
                            staff?.Id,

                        CheckInTime =
                            DateTime.Now,

                        Status =
                            "Rejected-Expired"
                    };

                _context.CheckIns.Add(
                    rejectedCheckIn);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    TempData["Error"] =
                        "The check-in could not be recorded.";

                    return RedirectToAction(
                        nameof(CheckIn));
                }

                TempData["Error"] =
                    $"Access denied. {member.FullName} does not have an active membership.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            /*
             * Repair subscriptions created before
             * visit tracking was added.
             *
             * This only repairs subscriptions where
             * TotalVisits was never initialized.
             */
            if (activeSub.TotalVisits <= 0 &&
                activeSub.MembershipPlan != null &&
                activeSub.MembershipPlan.TotalVisits > 0)
            {
                activeSub.TotalVisits =
                    activeSub.MembershipPlan.TotalVisits;

                activeSub.RemainingVisits =
                    activeSub.MembershipPlan.TotalVisits;

                await _context.SaveChangesAsync();
            }

            if (activeSub.RemainingVisits <= 0)
            {
                var rejectedCheckIn =
                    new CheckIn
                    {
                        MemberId =
                            member.Id,

                        StaffId =
                            staff?.Id,

                        CheckInTime =
                            DateTime.Now,

                        Status =
                            "Rejected-NoVisits"
                    };

                _context.CheckIns.Add(
                    rejectedCheckIn);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    TempData["Error"] =
                        "The check-in could not be recorded.";

                    return RedirectToAction(
                        nameof(CheckIn));
                }

                TempData["Error"] =
                    $"Access denied. {member.FullName} has no remaining visits.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            activeSub.RemainingVisits--;

            var checkIn =
                new CheckIn
                {
                    MemberId =
                        member.Id,

                    StaffId =
                        staff?.Id,

                    CheckInTime =
                        DateTime.Now,

                    Status =
                        "Allowed"
                };

            _context.CheckIns.Add(checkIn);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                TempData["Error"] =
                    "An error occurred while recording the check-in. Please try again.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            TempData["Success"] =
                $"Check-in successful. Welcome {member.FullName}! " +
                $"Remaining visits: {activeSub.RemainingVisits}";

            return RedirectToAction(
                nameof(CheckIn));
        }

        /*
         * Temporary administrative repair action.
         *
         * This is intended to repair an existing active
         * membership whose visit values were created before
         * the visit-tracking system was introduced.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetMemberVisits(
            string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                TempData["Error"] =
                    "Please enter the member phone number.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            phone = phone.Trim();

            var member =
                await _context.Members
                    .FirstOrDefaultAsync(
                        m => m.Phone == phone);

            if (member == null)
            {
                TempData["Error"] =
                    "Member was not found.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            var subscription =
                await _context.Subscriptions
                    .Include(s => s.MembershipPlan)
                    .Where(s =>
                        s.MemberId == member.Id &&
                        s.Status == "Active")
                    .OrderByDescending(
                        s => s.EndDate)
                    .FirstOrDefaultAsync();

            if (subscription == null)
            {
                TempData["Error"] =
                    $"No active membership was found for {member.FullName}.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            if (subscription.MembershipPlan == null)
            {
                TempData["Error"] =
                    "The membership plan for this subscription could not be found.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            if (subscription.MembershipPlan.TotalVisits <= 0)
            {
                TempData["Error"] =
                    "The membership plan does not have a valid visit limit.";

                return RedirectToAction(
                    nameof(CheckIn));
            }

            subscription.TotalVisits =
                subscription.MembershipPlan.TotalVisits;

            subscription.RemainingVisits =
                subscription.MembershipPlan.TotalVisits;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{member.FullName}'s membership has been reset to " +
                $"{subscription.RemainingVisits} visits.";

            return RedirectToAction(
                nameof(CheckIn));
        }

        [HttpGet]
        public async Task<IActionResult> SearchMember(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return View(new List<Member>());

            var members =
                await _context.Members
                    .Include(m => m.User)
                    .Include(m => m.Plan)
                    .Where(m =>
                        m.Phone.Contains(phone))
                    .OrderBy(m => m.FullName)
                    .Take(20)
                    .ToListAsync();

            return View(members);
        }
    }

}
