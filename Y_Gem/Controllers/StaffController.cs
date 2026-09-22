using Microsoft.AspNetCore.Authorization;
using Y_GYM.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Y_GYM.Models;
using Y_GYM.Repository;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Models.ViewModels;


namespace Y_GYM.Controllers
{
    // [Authorize(Roles = "Staff")]
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

        // GET: Staff Dashboard
        public  IActionResult Index()
        {
            //  TODO-TEMP: مؤقت للاختبار فقط
            var staff =  _context.Staff.Include(s => s.User).FirstOrDefault();
            // var userId = _userManager.GetUserId(User);
            // var staff =  _staffRepo.GetByUserId(userId);

            if (staff == null) return NotFound();
            return View(staff);
        }

        // ---------- تسجيل عضو جديد ----------
        [HttpGet]
        public IActionResult RegisterMember() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]//?
        public  IActionResult RegisterMember(RegisterMemberVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var newUser = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                PhoneNumber = vm.Phone
            };

            var result = _userManager.CreateAsync(newUser, vm.Password).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(vm);
            }

             _userManager.AddToRoleAsync(newUser, "Member").GetAwaiter().GetResult();

            var member = new Member
            {
                UserId = newUser.Id,
                Weight = vm.Weight,
                Height = vm.Height,
                Goal = vm.Goal,
                FitnessLevel = vm.FitnessLevel,
                QRCode = Guid.NewGuid().ToString()
            };

             _memberRepo.Add(member);
             _memberRepo.Save();

            TempData["Success"] = "The member has been successfully registered";
            return RedirectToAction(nameof(RegisterMember));
        }

        // ---------- تسجيل دفعة + إنشاء اشتراك ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult RegisterPayment(int memberId, int planId, double amountPaid)
        {
            var plan =  _context.MembershipPlans.Find(planId);
            if (plan == null)
            {
                TempData["Error"] = "The plan does not exist";
                return RedirectToAction(nameof(Index));//?
            }

            using var transaction =  _context.Database.BeginTransaction();//?
            try
            {
                var subscription = new Subscription
                {
                    MemberId = memberId,
                    PlanId = planId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(plan.DurationDays),
                    Status = "Active"
                };
                _context.Subscriptions.Add(subscription);
                 _context.SaveChanges();
                // ⚠️ TODO-TEMP: مؤقت للاختبار فقط
                var staffId = ( _staffRepo.GetAll()).First().Id;
                // var staffId = ( _staffRepo.GetByUserId(_userManager.GetUserId(User))).Id;
                var payment = new Payment
                {
                    SubscriptionId = subscription.Id,
                    Amount = (decimal)amountPaid,
                    PaymentDate = DateTime.Now,
                    StaffId = staffId
                };
                _context.Payments.Add(payment);
                 _context.SaveChanges();

                 transaction.Commit();
                TempData["Success"] = "Your payment and subscription have been successfully processed";
            }
            catch
            {
                 transaction.Rollback();
                TempData["Error"] = "An error occurred while processing the payment";
            }

            return RedirectToAction(nameof(Index));
        }

        // ---------- Check-in بالـ QR Code ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult CheckIn(string qrCode)
        {
            var member =  _context.Members
                .Include(m => m.User)
                .FirstOrDefault(m => m.QRCode == qrCode);

            if (member == null)
            {
                TempData["Error"] = "No member with this code was found";
                return RedirectToAction(nameof(Index));
            }

            var activeSub =  _memberRepo.GetActiveSubscription(member.Id);

            // ⚠️ TODO-TEMP: مؤقت للاختبار فقط
            var staffId = ( _staffRepo.GetAll()).First().Id;
            // var staffId = ( _staffRepo.GetByUserId(_userManager.GetUserId(User))).Id;

            var checkIn = new CheckIn
            {
                MemberId = member.Id,
                StaffId = staffId,
                CheckInTime = DateTime.Now,
                Status = activeSub != null ? "Allowed" : "Rejected-Expired"
            };

            _context.CheckIns.Add(checkIn);
             _context.SaveChanges();

            if (activeSub == null)
            {
                TempData["Error"] = $"Subscription expired - Access denied for {member.User.FullName}";
            }
            else
            {
                TempData["Success"] = $"Was allowed to enter - {member.User.FullName}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}