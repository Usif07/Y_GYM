using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Data;
using Y_GYM.Models;

namespace Y_GYM.Services.Chatbot
{
    public class ChatbotContextService : IChatbotContextService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatbotContextService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> BuildContextAsync(
            CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                return BuildGuestContext();
            }

            var user = httpContext.User;

            // =========================================================
            // GUEST
            // =========================================================

            if (user.Identity?.IsAuthenticated != true)
            {
                return await BuildGuestContextAsync(cancellationToken);
            }

            var userId = _userManager.GetUserId(user);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return await BuildGuestContextAsync(cancellationToken);
            }

            // =========================================================
            // ADMIN
            // =========================================================

            if (user.IsInRole("Admin"))
            {
                return await BuildAdminContextAsync(
                    userId,
                    cancellationToken);
            }

            // =========================================================
            // STAFF
            // =========================================================

            if (user.IsInRole("Staff"))
            {
                return await BuildStaffContextAsync(
                    userId,
                    cancellationToken);
            }

            // =========================================================
            // TRAINER
            // =========================================================

            if (user.IsInRole("Trainer"))
            {
                return await BuildTrainerContextAsync(
                    userId,
                    cancellationToken);
            }

            // =========================================================
            // MEMBER
            // =========================================================

            if (user.IsInRole("Member"))
            {
                return await BuildMemberContextAsync(
                    userId,
                    cancellationToken);
            }

            return await BuildGuestContextAsync(cancellationToken);
        }

        // =============================================================
        // GUEST CONTEXT
        // =============================================================

        private string BuildGuestContext()
        {
            return """
                   USER TYPE: Guest

                   The user is not logged in.

                   You may only discuss public Y_GYM information.
                   Do not claim to know personal member information.
                   Do not invent private information.

                   Public information includes:
                   - Membership plans
                   - Membership prices
                   - Membership duration
                   - Number of visits included in each plan
                   - General gym information

                   If the user asks about personal attendance,
                   bookings, subscription, payments, progress, diet plans,
                   or other private information, explain that they need
                   to log in to access their personal information.
                   """;
        }

        private async Task<string> BuildGuestContextAsync(
            CancellationToken cancellationToken)
        {
            var plans = await _context.MembershipPlans
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);

            var lines = new List<string>
            {
                "USER TYPE: Guest",
                "",
                "PUBLIC Y_GYM MEMBERSHIP PLANS:"
            };

            if (plans.Count == 0)
            {
                lines.Add("No membership plans are currently available.");
            }
            else
            {
                foreach (var plan in plans)
                {
                    lines.Add(
                        $"- {plan.Name}: " +
                        $"Price={plan.Price:0.##}, " +
                        $"Duration={plan.DurationDays} days, " +
                        $"Visits={plan.TotalVisits}");
                }
            }

            lines.Add("");
            lines.Add(
                "The user is a guest. Never expose private member data.");

            return string.Join(Environment.NewLine, lines);
        }

        // =============================================================
        // MEMBER CONTEXT
        // =============================================================

        private async Task<string> BuildMemberContextAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var member = await _context.Members
                .AsNoTracking()
                .Include(m => m.Plan)
                .FirstOrDefaultAsync(
                    m => m.UserId == userId,
                    cancellationToken);

            if (member == null)
            {
                return """
                       USER TYPE: Member

                       The authenticated account does not currently
                       have a Member profile.

                       Do not invent member information.
                       """;
            }

            var user = await _userManager.FindByIdAsync(userId);

            var activeSubscription = await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.MembershipPlan)
                .Where(s => s.MemberId == member.Id)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync(cancellationToken);

            var checkIns = await _context.CheckIns
                .AsNoTracking()
                .Where(c =>
                    c.MemberId == member.Id &&
                    c.Status == "Allowed")
                .OrderByDescending(c => c.CheckInTime)
                .ToListAsync(cancellationToken);

            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.ClassSchedule)
                    .ThenInclude(s => s.Class)
                .Include(b => b.ClassSchedule)
                    .ThenInclude(s => s.Coach)
                        .ThenInclude(c => c.User)
                .Where(b => b.MemberId == member.Id)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync(cancellationToken);

            var dietPlans = await _context.DietPlans
                .AsNoTracking()
                .Include(d => d.Coach)
                    .ThenInclude(c => c.User)
                .Where(d => d.MemberId == member.Id)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync(cancellationToken);

            var progress = await _context.ProgressLogs
                .AsNoTracking()
                .Where(p => p.MemberId == member.Id)
                .OrderByDescending(p => p.RecordDate)
                .ToListAsync(cancellationToken);

            var lines = new List<string>
            {
                "USER TYPE: Member",
                "",
                "IMPORTANT SECURITY RULE:",
                "This context belongs ONLY to the authenticated member.",
                "Never reveal information about another member.",
                "",
                "MEMBER PROFILE:",
                $"Name: {member.FullName}",
                $"Phone: {member.Phone}",
                $"Join Date: {member.JoinDate:yyyy-MM-dd}",
                $"Goal: {member.Goal ?? "Not specified"}",
                $"Fitness Level: {member.FitnessLevel ?? "Not specified"}",
                $"Weight: {member.Weight?.ToString() ?? "Not specified"}",
                $"Height: {member.Height?.ToString() ?? "Not specified"}",
                $"Username: {user?.UserName ?? "Not available"}"
            };

            // ---------------------------------------------------------
            // Subscription
            // ---------------------------------------------------------

            lines.Add("");
            lines.Add("CURRENT / LATEST SUBSCRIPTION:");

            if (activeSubscription == null)
            {
                lines.Add("No subscription found.");
            }
            else
            {
                lines.Add(
                    $"Plan: {activeSubscription.MembershipPlan?.Name ?? "Unknown"}");

                lines.Add(
                    $"Status: {activeSubscription.Status}");

                lines.Add(
                    $"Start Date: {activeSubscription.StartDate:yyyy-MM-dd}");

                lines.Add(
                    $"End Date: {activeSubscription.EndDate:yyyy-MM-dd}");

                lines.Add(
                    $"Total Visits: {activeSubscription.TotalVisits}");

                lines.Add(
                    $"Remaining Visits: {activeSubscription.RemainingVisits}");
            }

            // ---------------------------------------------------------
            // Attendance
            // ---------------------------------------------------------

            lines.Add("");
            lines.Add("ATTENDANCE:");

            lines.Add(
                $"Total allowed check-ins: {checkIns.Count}");

            if (checkIns.Count > 0)
            {
                lines.Add("Recent check-ins:");

                foreach (var checkIn in checkIns.Take(10))
                {
                    lines.Add(
                        $"- {checkIn.CheckInTime:yyyy-MM-dd HH:mm}");
                }
            }

            // ---------------------------------------------------------
            // Bookings / Sessions / Coaches
            // ---------------------------------------------------------

            lines.Add("");
            lines.Add("CLASS BOOKINGS:");

            if (bookings.Count == 0)
            {
                lines.Add("No bookings found.");
            }
            else
            {
                lines.Add(
                    $"Total bookings: {bookings.Count}");

                var attendedBookings =
                    bookings.Count(b => b.IsAttended);

                lines.Add(
                    $"Attended booked classes: {attendedBookings}");

                foreach (var booking in bookings.Take(15))
                {
                    var className =
                        booking.ClassSchedule?.Class?.Name
                        ?? "Unknown class";

                    var coachName =
                        booking.ClassSchedule?.Coach?.User?.FullName
                        ?? "Unknown trainer";

                    lines.Add(
                        $"- Class: {className}, " +
                        $"Trainer: {coachName}, " +
                        $"Start: {booking.ClassSchedule?.StartTime:yyyy-MM-dd HH:mm}, " +
                        $"Booking Status: {booking.Status}, " +
                        $"Attended: {booking.IsAttended}");
                }
            }

            // ---------------------------------------------------------
            // Diet Plans
            // ---------------------------------------------------------

            lines.Add("");
            lines.Add("DIET PLANS:");

            if (dietPlans.Count == 0)
            {
                lines.Add("No diet plans found.");
            }
            else
            {
                foreach (var diet in dietPlans.Take(10))
                {
                    var coachName =
                        diet.Coach?.User?.FullName
                        ?? "Unknown trainer";

                    lines.Add(
                        $"- Start: {diet.StartDate:yyyy-MM-dd}, " +
                        $"Trainer: {coachName}, " +
                        $"Details: {diet.PlanDetails ?? "No details"}");
                }
            }

            // ---------------------------------------------------------
            // Progress
            // ---------------------------------------------------------

            lines.Add("");
            lines.Add("PROGRESS:");

            if (progress.Count == 0)
            {
                lines.Add("No progress records found.");
            }
            else
            {
                foreach (var record in progress.Take(10))
                {
                    lines.Add(
                        $"- Date: {record.RecordDate:yyyy-MM-dd}, " +
                        $"Weight: {record.Weight}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        // =============================================================
        // TRAINER CONTEXT
        // =============================================================

        private async Task<string> BuildTrainerContextAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var coach = await _context.Coaches
                .AsNoTracking()
                .Include(c => c.User)
                .FirstOrDefaultAsync(
                    c => c.UserId == userId,
                    cancellationToken);

            if (coach == null)
            {
                return """
                       USER TYPE: Trainer

                       No trainer profile was found for this account.
                       Do not invent trainer information.
                       """;
            }

            var schedules = await _context.ClassSchedules
                .AsNoTracking()
                .Include(s => s.Class)
                .Where(s => s.CoachId == coach.Id)
                .OrderBy(s => s.StartTime)
                .ToListAsync(cancellationToken);

            var dietPlans = await _context.DietPlans
                .AsNoTracking()
                .Include(d => d.Member)
                .Where(d => d.CoachId == coach.Id)
                .OrderByDescending(d => d.StartDate)
                .ToListAsync(cancellationToken);

            var lines = new List<string>
            {
                "USER TYPE: Trainer",
                "",
                "TRAINER PROFILE:",
                $"Name: {coach.User?.FullName ?? "Unknown"}",
                $"Specialty: {coach.CoachSpecialty}",
                "",
                "TRAINER SCHEDULES:"
            };

            if (schedules.Count == 0)
            {
                lines.Add("No schedules found.");
            }
            else
            {
                foreach (var schedule in schedules.Take(20))
                {
                    lines.Add(
                        $"- Class: {schedule.Class?.Name ?? "Unknown"}, " +
                        $"Start: {schedule.StartTime:yyyy-MM-dd HH:mm}, " +
                        $"End: {schedule.EndTime:yyyy-MM-dd HH:mm}, " +
                        $"Available Places: {schedule.AvailablePlaces}");
                }
            }

            lines.Add("");
            lines.Add("DIET PLANS ASSIGNED TO THIS TRAINER:");

            if (dietPlans.Count == 0)
            {
                lines.Add("No diet plans found.");
            }
            else
            {
                foreach (var diet in dietPlans.Take(20))
                {
                    lines.Add(
                        $"- Member: {diet.Member?.FullName ?? "Unknown"}, " +
                        $"Start: {diet.StartDate:yyyy-MM-dd}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        // =============================================================
        // STAFF CONTEXT
        // =============================================================

        private async Task<string> BuildStaffContextAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var memberCount = await _context.Members
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var activeSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(
                    s => s.Status == "Active",
                    cancellationToken);

            var todayCheckIns = await _context.CheckIns
                .AsNoTracking()
                .CountAsync(
                    c => c.CheckInTime >= DateTime.Today &&
                         c.Status == "Allowed",
                    cancellationToken);

            var plans = await _context.MembershipPlans
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);

            var lines = new List<string>
            {
                "USER TYPE: Staff",
                "",
                "STAFF DASHBOARD INFORMATION:",
                $"Total members: {memberCount}",
                $"Active subscriptions: {activeSubscriptions}",
                $"Today's allowed check-ins: {todayCheckIns}",
                "",
                "AVAILABLE MEMBERSHIP PLANS:"
            };

            foreach (var plan in plans)
            {
                lines.Add(
                    $"- {plan.Name}: " +
                    $"Price={plan.Price:0.##}, " +
                    $"Duration={plan.DurationDays} days, " +
                    $"Visits={plan.TotalVisits}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        // =============================================================
        // ADMIN CONTEXT
        // =============================================================

        private async Task<string> BuildAdminContextAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var memberCount = await _context.Members
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var staffCount = await _context.Staff
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var coachCount = await _context.Coaches
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var activeSubscriptions = await _context.Subscriptions
                .AsNoTracking()
                .CountAsync(
                    s => s.Status == "Active",
                    cancellationToken);

            var todayCheckIns = await _context.CheckIns
                .AsNoTracking()
                .CountAsync(
                    c => c.CheckInTime >= DateTime.Today &&
                         c.Status == "Allowed",
                    cancellationToken);

            var paymentsToday = await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentDate >= DateTime.Today)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken)
                ?? 0;

            var plans = await _context.MembershipPlans
                .AsNoTracking()
                .OrderBy(p => p.Price)
                .ToListAsync(cancellationToken);

            var lines = new List<string>
            {
                "USER TYPE: Admin",
                "",
                "ADMIN SYSTEM SUMMARY:",
                $"Total members: {memberCount}",
                $"Total staff: {staffCount}",
                $"Total trainers: {coachCount}",
                $"Active subscriptions: {activeSubscriptions}",
                $"Today's allowed check-ins: {todayCheckIns}",
                $"Today's payments: {paymentsToday:0.00}",
                "",
                "MEMBERSHIP PLANS:"
            };

            foreach (var plan in plans)
            {
                lines.Add(
                    $"- {plan.Name}: " +
                    $"Price={plan.Price:0.##}, " +
                    $"Duration={plan.DurationDays} days, " +
                    $"Visits={plan.TotalVisits}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}