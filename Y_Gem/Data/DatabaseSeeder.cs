using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Y_GYM.Models;

namespace Y_GYM.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // ==========================================
            // ROLES
            // ==========================================

            string[] roles =
            {
                "Admin",
                "Staff",
                "Trainer",
                "Member"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }

            // ==========================================
            // ADMIN
            // ==========================================

            var admin = await userManager.FindByNameAsync("admin");

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@fitnessgym.local",
                    FullName = "FITNESS GYM Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    "Admin123!");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Could not create admin user.");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(
                    admin,
                    "Admin");
            }

            // ==========================================
            // STAFF USER
            // ==========================================

            var staffUser =
                await userManager.FindByNameAsync("staff1");

            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = "staff1",
                    Email = "staff1@fitnessgym.local",
                    FullName = "Ahmed Staff",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    staffUser,
                    "Staff123!");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Could not create staff user.");
                }
            }

            if (!await userManager.IsInRoleAsync(
                    staffUser,
                    "Staff"))
            {
                await userManager.AddToRoleAsync(
                    staffUser,
                    "Staff");
            }

            // ==========================================
            // STAFF PROFILE
            // ==========================================

            var staff = await context.Staff
                .FirstOrDefaultAsync(
                    s => s.UserId == staffUser.Id);

            if (staff == null)
            {
                staff = new Staff
                {
                    UserId = staffUser.Id,
                    JobTitle = "Reception Staff",
                    ShiftTime = "Morning"
                };

                context.Staff.Add(staff);

                await context.SaveChangesAsync();
            }

            // ==========================================
            // MEMBERSHIP PLANS
            // ==========================================

            var basicPlan = await context.MembershipPlans
                .FirstOrDefaultAsync(
                    p => p.Name == "Basic");

            if (basicPlan == null)
            {
                basicPlan = new MembershipPlan
                {
                    Name = "Basic",
                    Price = 300,
                    DurationDays = 30
                };

                context.MembershipPlans.Add(basicPlan);
            }

            var monthlyPlan = await context.MembershipPlans
                .FirstOrDefaultAsync(
                    p => p.Name == "Monthly");

            if (monthlyPlan == null)
            {
                monthlyPlan = new MembershipPlan
                {
                    Name = "Monthly",
                    Price = 500,
                    DurationDays = 30
                };

                context.MembershipPlans.Add(monthlyPlan);
            }

            var quarterlyPlan = await context.MembershipPlans
                .FirstOrDefaultAsync(
                    p => p.Name == "Quarterly");

            if (quarterlyPlan == null)
            {
                quarterlyPlan = new MembershipPlan
                {
                    Name = "Quarterly",
                    Price = 1200,
                    DurationDays = 90
                };

                context.MembershipPlans.Add(quarterlyPlan);
            }

            await context.SaveChangesAsync();

            // ==========================================
            // MEMBERS
            // ==========================================

            var membersData = new[]
            {
                new
                {
                    Username = "member1",
                    Email = "member1@fitnessgym.local",
                    Name = "Ahmed Mohamed",
                    Phone = "01000000001",
                    PlanId = monthlyPlan.Id
                },

                new
                {
                    Username = "member2",
                    Email = "member2@fitnessgym.local",
                    Name = "Mohamed Ali",
                    Phone = "01000000002",
                    PlanId = quarterlyPlan.Id
                },

                new
                {
                    Username = "member3",
                    Email = "member3@fitnessgym.local",
                    Name = "Omar Hassan",
                    Phone = "01000000003",
                    PlanId = basicPlan.Id
                },

                new
                {
                    Username = "member4",
                    Email = "member4@fitnessgym.local",
                    Name = "Youssef Samir",
                    Phone = "01000000004",
                    PlanId = monthlyPlan.Id
                },

                new
                {
                    Username = "member5",
                    Email = "member5@fitnessgym.local",
                    Name = "Karim Adel",
                    Phone = "01000000005",
                    PlanId = basicPlan.Id
                }
            };

            var members = new List<Member>();

            foreach (var data in membersData)
            {
                var user = await userManager
                    .FindByNameAsync(data.Username);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = data.Username,
                        Email = data.Email,
                        FullName = data.Name,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(
                        user,
                        "Member123!");

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create {data.Username}");
                    }
                }

                if (!await userManager.IsInRoleAsync(
                        user,
                        "Member"))
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Member");
                }

                var member = await context.Members
                    .FirstOrDefaultAsync(
                        m => m.UserId == user.Id);

                if (member == null)
                {
                    member = new Member
                    {
                        FullName = data.Name,
                        Phone = data.Phone,
                        JoinDate = DateTime.Today.AddDays(-30),
                        MembershipPlanId = data.PlanId,
                        UserId = user.Id,
                        FitnessLevel = "Intermediate",
                        Goal = "General Fitness"
                    };

                    context.Members.Add(member);
                    await context.SaveChangesAsync();
                }

                members.Add(member);
            }

            // ==========================================
            // SUBSCRIPTIONS
            // ==========================================

            await CreateSubscriptionIfMissing(
                context,
                members[0],
                monthlyPlan,
                DateTime.Today.AddDays(-10),
                DateTime.Today.AddDays(20),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[1],
                quarterlyPlan,
                DateTime.Today.AddDays(-20),
                DateTime.Today.AddDays(70),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[2],
                basicPlan,
                DateTime.Today.AddDays(-60),
                DateTime.Today.AddDays(-30),
                "Expired");

            await CreateSubscriptionIfMissing(
                context,
                members[3],
                monthlyPlan,
                DateTime.Today.AddDays(5),
                DateTime.Today.AddDays(35),
                "Pending");

            await CreateSubscriptionIfMissing(
                context,
                members[4],
                basicPlan,
                DateTime.Today.AddDays(-5),
                DateTime.Today.AddDays(25),
                "Active");

            // ==========================================
            // PAYMENTS
            // ==========================================

            var subscriptions =
                await context.Subscriptions
                    .Include(s => s.MembershipPlan)
                    .Where(s =>
                        members.Select(m => m.Id)
                            .Contains(s.MemberId))
                    .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                var exists = await context.Payments
                    .AnyAsync(p =>
                        p.SubscriptionId ==
                        subscription.Id);

                if (!exists && subscription.MembershipPlan != null)
                {
                    context.Payments.Add(new Payment
                    {
                        MemberId = subscription.MemberId,
                        SubscriptionId = subscription.Id,
                        StaffId = staff.Id,
                        Amount = (decimal)
                            subscription.MembershipPlan.Price,
                        PaymentDate =
                            subscription.StartDate
                    });
                }
            }

            await context.SaveChangesAsync();

            // ==========================================
            // CHECK INS
            // ==========================================

            var hasCheckIns =
                await context.CheckIns.AnyAsync();

            if (!hasCheckIns)
            {
                // Allowed
                context.CheckIns.Add(new CheckIn
                {
                    MemberId = members[0].Id,
                    StaffId = staff.Id,
                    CheckInTime = DateTime.Now.AddHours(-2),
                    Status = "Allowed"
                });

                // Allowed
                context.CheckIns.Add(new CheckIn
                {
                    MemberId = members[1].Id,
                    StaffId = staff.Id,
                    CheckInTime = DateTime.Now.AddHours(-1),
                    Status = "Allowed"
                });

                // Expired
                context.CheckIns.Add(new CheckIn
                {
                    MemberId = members[2].Id,
                    StaffId = staff.Id,
                    CheckInTime = DateTime.Now.AddDays(-2),
                    Status = "Rejected-Expired"
                });

                // Pending
                context.CheckIns.Add(new CheckIn
                {
                    MemberId = members[3].Id,
                    StaffId = staff.Id,
                    CheckInTime = DateTime.Now.AddMinutes(-30),
                    Status = "Rejected-Pending"
                });

                // Allowed
                context.CheckIns.Add(new CheckIn
                {
                    MemberId = members[4].Id,
                    StaffId = staff.Id,
                    CheckInTime = DateTime.Now.AddMinutes(-15),
                    Status = "Allowed"
                });

                await context.SaveChangesAsync();
            }
        }

        // ==========================================
        // SUBSCRIPTION HELPER
        // ==========================================

        private static async Task CreateSubscriptionIfMissing(
            ApplicationDbContext context,
            Member member,
            MembershipPlan plan,
            DateTime startDate,
            DateTime endDate,
            string status)
        {
            var exists = await context.Subscriptions
                .AnyAsync(s =>
                    s.MemberId == member.Id &&
                    s.PlanId == plan.Id);

            if (!exists)
            {
                context.Subscriptions.Add(new Subscription
                {
                    MemberId = member.Id,
                    PlanId = plan.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status
                });

                await context.SaveChangesAsync();
            }
        }
    }
}