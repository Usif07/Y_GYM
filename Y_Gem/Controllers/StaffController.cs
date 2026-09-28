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
        private readonly IStaffRepository _staffRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StaffController(
            IStaffRepository staffRepository,
            IMemberRepository memberRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _staffRepository = staffRepository;
            _memberRepository = memberRepository;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // STAFF DASHBOARD
        // ============================================================

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            Staff? currentStaff = null;

            if (!User.IsInRole("Admin") && !string.IsNullOrEmpty(currentUserId))
            {
                currentStaff = await _context.Staff
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.UserId == currentUserId);
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var totalMembers = await _context.Members.CountAsync();

            var activeMembers = await _context.Subscriptions
                .Where(s =>
                    s.Status == "Active" &&
                    s.StartDate <= today &&
                    s.EndDate >= today)
                .Select(s => s.MemberId)
                .Distinct()
                .CountAsync();

            var todayCheckIns = await _context.CheckIns
                .CountAsync(c =>
                    c.CheckInTime >= today &&
                    c.CheckInTime < tomorrow &&
                    c.Status == "Allowed");

            var todayPayments = await _context.Payments
                .Where(p =>
                    p.PaymentDate >= today &&
                    p.PaymentDate < tomorrow)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var recentCheckIns = await _context.CheckIns
                .Include(c => c.Member)
                    .ThenInclude(m => m.User)
                .Include(c => c.Staff)
                    .ThenInclude(s => s.User)
                .OrderByDescending(c => c.CheckInTime)
                .Take(8)
                .ToListAsync();

            var recentPayments = await _context.Payments
                .Include(p => p.Member)
                    .ThenInclude(m => m.User)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.MembershipPlan)
                .Include(p => p.Staff)
                    .ThenInclude(s => s.User)
                .OrderByDescending(p => p.PaymentDate)
                .Take(8)
                .ToListAsync();

            var vm = new StaffDashboardVM
            {
                Staff = currentStaff,
                IsAdmin = User.IsInRole("Admin"),
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                TodayCheckIns = todayCheckIns,
                TodayPayments = todayPayments,
                RecentCheckIns = recentCheckIns,
                RecentPayments = recentPayments
            };

            return View(vm);
        }

        // ============================================================
        // ADMIN - STAFF LIST
        // ============================================================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .OrderBy(s => s.User.FullName)
                .ToListAsync();

            return View(staff);
        }

        // ============================================================
        // ADMIN - CREATE STAFF - GET
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // ADMIN - CREATE STAFF - POST
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string fullName,
            string userName,
            string email,
            string password,
            string phone,
            string jobTitle,
            string shiftTime)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            password = password ?? string.Empty;
            phone = phone?.Trim() ?? string.Empty;
            jobTitle = jobTitle?.Trim() ?? string.Empty;
            shiftTime = shiftTime?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(jobTitle) ||
                string.IsNullOrWhiteSpace(shiftTime))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please fill in all required fields.");

                return View();
            }

            var existingUserName = await _userManager.FindByNameAsync(userName);

            if (existingUserName != null)
            {
                ModelState.AddModelError(
                    "userName",
                    "This username is already in use.");

                return View();
            }

            var existingEmail = await _userManager.FindByEmailAsync(email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(
                    "email",
                    "This email is already in use.");

                return View();
            }

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
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View();
            }

            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(
                    new IdentityRole("Staff"));
            }

            var roleResult =
                await _userManager.AddToRoleAsync(user, "Staff");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View();
            }

            var staff = new Staff
            {
                UserId = user.Id,
                JobTitle = jobTitle,
                ShiftTime = shiftTime
            };

            try
            {
                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();
            }
            catch
            {
                await _userManager.DeleteAsync(user);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create the staff profile.");

                return View();
            }

            TempData["SuccessMessage"] =
                "Staff account created successfully.";

            return RedirectToAction(nameof(Manage));
        }

        // ============================================================
        // ADMIN - EDIT STAFF - GET
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // ============================================================
        // ADMIN - EDIT STAFF - POST
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string fullName,
            string email,
            string phone,
            string jobTitle,
            string shiftTime)
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                return NotFound();
            }

            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            phone = phone?.Trim() ?? string.Empty;
            jobTitle = jobTitle?.Trim() ?? string.Empty;
            shiftTime = shiftTime?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(jobTitle) ||
                string.IsNullOrWhiteSpace(shiftTime))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please fill in all required fields.");

                return View(staff);
            }

            var emailOwner = await _userManager.FindByEmailAsync(email);

            if (emailOwner != null &&
                emailOwner.Id != staff.UserId)
            {
                ModelState.AddModelError(
                    "email",
                    "This email is already in use.");

                return View(staff);
            }

            staff.User.FullName = fullName;
            staff.User.Email = email;
            staff.User.PhoneNumber = phone;

            staff.JobTitle = jobTitle;
            staff.ShiftTime = shiftTime;

            var updateResult =
                await _userManager.UpdateAsync(staff.User);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(staff);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Staff information updated successfully.";

            return RedirectToAction(nameof(Manage));
        }

        // ============================================================
        // ADMIN - DELETE STAFF - GET
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // ============================================================
        // ADMIN - DELETE STAFF - POST
        // ============================================================

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Staff
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff == null)
            {
                return NotFound();
            }

            var user = staff.User;

            _context.Staff.Remove(staff);
            await _context.SaveChangesAsync();

            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }

            TempData["SuccessMessage"] =
                "Staff account deleted successfully.";

            return RedirectToAction(nameof(Manage));
        }

        // ============================================================
        // STAFF - REGISTER MEMBER
        // ============================================================

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public IActionResult RegisterMember()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterMember(
            string fullName,
            string username,
            string email,
            string phone,
            double? weight,
            double? height,
            string? goal,
            string? fitnessLevel)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            username = username?.Trim() ?? string.Empty;
            email = email?.Trim() ?? string.Empty;
            phone = phone?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please fill in all required fields.");

                return View();
            }

            if (await _userManager.FindByNameAsync(username) != null)
            {
                ModelState.AddModelError(
                    "username",
                    "This username is already in use.");

                return View();
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(
                    "email",
                    "This email is already in use.");

                return View();
            }

            var phoneExists = await _context.Members
                .AnyAsync(m => m.Phone == phone);

            if (phoneExists)
            {
                ModelState.AddModelError(
                    "phone",
                    "This phone number is already registered.");

                return View();
            }

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                PhoneNumber = phone,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result =
                await _userManager.CreateAsync(user, "Member@123");

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            var member = new Member
            {
                FullName = fullName,
                Phone = phone,
                Weight = weight,
                Height = height,
                Goal = goal,
                FitnessLevel = fitnessLevel,
                UserId = user.Id
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Member registered successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // STAFF - REGISTER PAYMENT
        // ============================================================

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> RegisterPayment()
        {
            ViewBag.Plans = await _context.MembershipPlans
                .OrderBy(p => p.Price)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPayment(
            string phone,
            int planId,
            decimal amountPaid)
        {
            phone = phone?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError(
                    "phone",
                    "Please enter the member phone number.");
            }

            if (amountPaid <= 0)
            {
                ModelState.AddModelError(
                    "amountPaid",
                    "Amount must be greater than zero.");
            }

            var plan = await _context.MembershipPlans
                .FirstOrDefaultAsync(p => p.Id == planId);

            if (plan == null)
            {
                ModelState.AddModelError(
                    "planId",
                    "Selected membership plan was not found.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Plans = await _context.MembershipPlans
                    .OrderBy(p => p.Price)
                    .ToListAsync();

                return View();
            }

            var member = await _context.Members
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Phone == phone);

            if (member == null)
            {
                ModelState.AddModelError(
                    "phone",
                    "Member was not found.");

                ViewBag.Plans = await _context.MembershipPlans
                    .OrderBy(p => p.Price)
                    .ToListAsync();

                return View();
            }

            var staffUserId = _userManager.GetUserId(User);

            Staff? staff = null;

            if (!User.IsInRole("Admin") &&
                !string.IsNullOrEmpty(staffUserId))
            {
                staff = await _context.Staff
                    .FirstOrDefaultAsync(s =>
                        s.UserId == staffUserId);
            }

            var existingSubscription =
                await _context.Subscriptions
                    .FirstOrDefaultAsync(s =>
                        s.MemberId == member.Id &&
                        s.Status == "Pending");

            var strategy =
                _context.Database.CreateExecutionStrategy();

            decimal changeAmount = 0;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                if (existingSubscription != null)
                {
                    var remainingAmount =
                        (decimal)(existingSubscription.MembershipPlan?.Price ?? plan!.Price);

                    var paidAlready = await _context.Payments
                        .Where(p =>
                            p.SubscriptionId ==
                            existingSubscription.Id)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    remainingAmount -= paidAlready;

                    var payment = new Payment
                    {
                        MemberId = member.Id,
                        SubscriptionId =
                            existingSubscription.Id,
                        StaffId = staff?.Id,
                        Amount = amountPaid,
                        PaymentDate = DateTime.Now
                    };

                    _context.Payments.Add(payment);

                    var totalPaid =
                        paidAlready + amountPaid;

                    if (totalPaid >= remainingAmount + paidAlready)
                    {
                        existingSubscription.Status = "Active";
                    }

                    changeAmount =
                        amountPaid > remainingAmount
                            ? amountPaid - remainingAmount
                            : 0;
                }
                else
                {
                    var subscription = new Subscription
                    {
                        MemberId = member.Id,
                        PlanId = plan!.Id,
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(
                            plan.DurationDays),
                        Status = amountPaid >=
                                 (decimal)plan.Price
                            ? "Active"
                            : "Pending",
                        TotalVisits = plan.TotalVisits,
                        RemainingVisits = plan.TotalVisits
                    };

                    _context.Subscriptions.Add(subscription);

                    await _context.SaveChangesAsync();

                    var payment = new Payment
                    {
                        MemberId = member.Id,
                        SubscriptionId =
                            subscription.Id,
                        StaffId = staff?.Id,
                        Amount = amountPaid,
                        PaymentDate = DateTime.Now
                    };

                    _context.Payments.Add(payment);

                    changeAmount =
                        amountPaid > (decimal)plan.Price
                            ? amountPaid - (decimal)plan.Price
                            : 0;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            });

            TempData["SuccessMessage"] =
                $"Payment registered successfully. Change: {changeAmount:0.00}";

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // STAFF - CHECK IN
        // ============================================================

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public IActionResult CheckIn()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string phone)
        {
            phone = phone?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError(
                    "phone",
                    "Please enter the member phone number.");

                return View();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Phone == phone);

            if (member == null)
            {
                ModelState.AddModelError(
                    "phone",
                    "Member was not found.");

                return View();
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .Where(s =>
                    s.MemberId == member.Id &&
                    s.Status == "Active" &&
                    s.StartDate <= DateTime.Today &&
                    s.EndDate >= DateTime.Today)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            var staffUserId = _userManager.GetUserId(User);

            int? staffId = null;

            if (!User.IsInRole("Admin") &&
                !string.IsNullOrEmpty(staffUserId))
            {
                staffId = await _context.Staff
                    .Where(s => s.UserId == staffUserId)
                    .Select(s => (int?)s.Id)
                    .FirstOrDefaultAsync();
            }

            if (subscription == null)
            {
                _context.CheckIns.Add(new CheckIn
                {
                    MemberId = member.Id,
                    StaffId = staffId,
                    CheckInTime = DateTime.Now,
                    Status = "Rejected-Expired"
                });

                await _context.SaveChangesAsync();

                ModelState.AddModelError(
                    "phone",
                    "Member does not have an active subscription.");

                return View();
            }

            if (subscription.TotalVisits <= 0 &&
                subscription.MembershipPlan != null)
            {
                subscription.TotalVisits =
                    subscription.MembershipPlan.TotalVisits;

                subscription.RemainingVisits =
                    subscription.MembershipPlan.TotalVisits;
            }

            if (subscription.RemainingVisits <= 0)
            {
                _context.CheckIns.Add(new CheckIn
                {
                    MemberId = member.Id,
                    StaffId = staffId,
                    CheckInTime = DateTime.Now,
                    Status = "Rejected-NoVisits"
                });

                await _context.SaveChangesAsync();

                ModelState.AddModelError(
                    "phone",
                    "Member has no remaining visits.");

                return View();
            }

            subscription.RemainingVisits--;

            _context.CheckIns.Add(new CheckIn
            {
                MemberId = member.Id,
                StaffId = staffId,
                CheckInTime = DateTime.Now,
                Status = "Allowed"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Check-in allowed. Remaining visits: {subscription.RemainingVisits}";

            return RedirectToAction(nameof(CheckIn));
        }

        // ============================================================
        // STAFF - RESET MEMBER VISITS
        // ============================================================

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetMemberVisits(string phone)
        {
            phone = phone?.Trim() ?? string.Empty;

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Phone == phone);

            if (member == null)
            {
                TempData["ErrorMessage"] =
                    "Member was not found.";

                return RedirectToAction(nameof(CheckIn));
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.MembershipPlan)
                .Where(s =>
                    s.MemberId == member.Id &&
                    s.Status == "Active")
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (subscription == null ||
                subscription.MembershipPlan == null)
            {
                TempData["ErrorMessage"] =
                    "Active subscription was not found.";

                return RedirectToAction(nameof(CheckIn));
            }

            subscription.TotalVisits =
                subscription.MembershipPlan.TotalVisits;

            subscription.RemainingVisits =
                subscription.MembershipPlan.TotalVisits;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Member visits have been reset successfully.";

            return RedirectToAction(nameof(CheckIn));
        }

        // ============================================================
        // STAFF - SEARCH MEMBER
        // ============================================================

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> SearchMember(string phone)
        {
            phone = phone?.Trim() ?? string.Empty;

            var members = await _context.Members
                .Include(m => m.User)
                .Include(m => m.Plan)
                .Where(m => string.IsNullOrEmpty(phone) ||
                            m.Phone.Contains(phone))
                .OrderBy(m => m.FullName)
                .Take(20)
                .ToListAsync();

            return View(members);
        }
    }
}
