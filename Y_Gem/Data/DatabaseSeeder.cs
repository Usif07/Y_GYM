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
            // Existing account - DO NOT CHANGE
            // Username: admin
            // Password: Admin123!
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
            // EXISTING STAFF USER
            // DO NOT CHANGE
            // Username: staff1
            // Password: Staff123!
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
            // EXISTING STAFF PROFILE
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
            // EXISTING TRAINER USER
            // DO NOT CHANGE
            // Username: coach1
            // Password: Trainer123!
            // ==========================================

            var coachUser =
                await userManager.FindByNameAsync("coach1");

            if (coachUser == null)
            {
                coachUser = new ApplicationUser
                {
                    UserName = "coach1",
                    Email = "coach1@fitnessgym.local",
                    FullName = "Hassan Trainer",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    coachUser,
                    "Trainer123!");

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "Could not create trainer user.");
                }
            }

            if (!await userManager.IsInRoleAsync(
                    coachUser,
                    "Trainer"))
            {
                await userManager.AddToRoleAsync(
                    coachUser,
                    "Trainer");
            }

            // ==========================================
            // EXISTING COACH PROFILE
            // ==========================================

            var coach = await context.Coaches
                .FirstOrDefaultAsync(
                    c => c.UserId == coachUser.Id);

            if (coach == null)
            {
                coach = new Coach
                {
                    UserId = coachUser.Id,
                    CoachSpecialty = "Strength Training"
                };

                context.Coaches.Add(coach);

                await context.SaveChangesAsync();
            }

            // ==========================================
            // ADDITIONAL STAFF USERS
            // ==========================================

            var additionalStaffData = new[]
            {
                new
                {
                    Username = "staff2",
                    Email = "staff2@fitnessgym.local",
                    Name = "Omar Khaled",
                    Phone = "01010000002",
                    JobTitle = "Reception Staff",
                    ShiftTime = "Evening"
                },

                new
                {
                    Username = "staff3",
                    Email = "staff3@fitnessgym.local",
                    Name = "Karim Hassan",
                    Phone = "01010000003",
                    JobTitle = "Front Desk",
                    ShiftTime = "Night"
                }
            };

            foreach (var data in additionalStaffData)
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
                        PhoneNumber = data.Phone,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(
                        user,
                        "Staff123!");

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create {data.Username}");
                    }
                }

                if (!await userManager.IsInRoleAsync(
                        user,
                        "Staff"))
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Staff");
                }

                var staffProfile = await context.Staff
                    .FirstOrDefaultAsync(
                        s => s.UserId == user.Id);

                if (staffProfile == null)
                {
                    context.Staff.Add(new Staff
                    {
                        UserId = user.Id,
                        JobTitle = data.JobTitle,
                        ShiftTime = data.ShiftTime
                    });
                }
            }

            await context.SaveChangesAsync();

            // ==========================================
            // ADDITIONAL TRAINERS
            // ==========================================

            var additionalTrainersData = new[]
            {
                new
                {
                    Username = "coach2",
                    Email = "coach2@fitnessgym.local",
                    Name = "Ahmed Hassan",
                    Phone = "01020000002",
                    Specialty = "Cardio & Fat Loss"
                },

                new
                {
                    Username = "coach3",
                    Email = "coach3@fitnessgym.local",
                    Name = "Mohamed Adel",
                    Phone = "01020000003",
                    Specialty = "CrossFit"
                },

                new
                {
                    Username = "coach4",
                    Email = "coach4@fitnessgym.local",
                    Name = "Omar Samir",
                    Phone = "01020000004",
                    Specialty = "Bodybuilding"
                },

                new
                {
                    Username = "coach5",
                    Email = "coach5@fitnessgym.local",
                    Name = "Karim Mostafa",
                    Phone = "01020000005",
                    Specialty = "Yoga & Mobility"
                }
            };

            var additionalCoaches = new List<Coach>();

            foreach (var data in additionalTrainersData)
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
                        PhoneNumber = data.Phone,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(
                        user,
                        "Trainer123!");

                    if (!result.Succeeded)
                    {
                        throw new Exception(
                            $"Could not create {data.Username}");
                    }
                }

                if (!await userManager.IsInRoleAsync(
                        user,
                        "Trainer"))
                {
                    await userManager.AddToRoleAsync(
                        user,
                        "Trainer");
                }

                var trainer = await context.Coaches
                    .FirstOrDefaultAsync(
                        c => c.UserId == user.Id);

                if (trainer == null)
                {
                    trainer = new Coach
                    {
                        UserId = user.Id,
                        CoachSpecialty = data.Specialty
                    };

                    context.Coaches.Add(trainer);

                    await context.SaveChangesAsync();
                }

                additionalCoaches.Add(trainer);
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

            // NEW PLAN
            var halfYearPlan = await context.MembershipPlans
                .FirstOrDefaultAsync(
                    p => p.Name == "Half Year");

            if (halfYearPlan == null)
            {
                halfYearPlan = new MembershipPlan
                {
                    Name = "Half Year",
                    Price = 2100,
                    DurationDays = 180
                };

                context.MembershipPlans.Add(halfYearPlan);
            }

            // NEW PLAN
            var annualPlan = await context.MembershipPlans
                .FirstOrDefaultAsync(
                    p => p.Name == "Annual");

            if (annualPlan == null)
            {
                annualPlan = new MembershipPlan
                {
                    Name = "Annual",
                    Price = 3600,
                    DurationDays = 365
                };

                context.MembershipPlans.Add(annualPlan);
            }

            await context.SaveChangesAsync();

            // ==========================================
            // EXISTING MEMBERS
            // ==========================================

            var membersData = new[]
            {
                new
                {
                    Username = "member1",
                    Email = "member1@fitnessgym.local",
                    Name = "Ahmed Mohamed",
                    Phone = "01000000001",
                    PlanId = monthlyPlan.Id,
                    FitnessLevel = "Intermediate",
                    Goal = "General Fitness"
                },

                new
                {
                    Username = "member2",
                    Email = "member2@fitnessgym.local",
                    Name = "Mohamed Ali",
                    Phone = "01000000002",
                    PlanId = quarterlyPlan.Id,
                    FitnessLevel = "Advanced",
                    Goal = "Muscle Gain"
                },

                new
                {
                    Username = "member3",
                    Email = "member3@fitnessgym.local",
                    Name = "Omar Hassan",
                    Phone = "01000000003",
                    PlanId = basicPlan.Id,
                    FitnessLevel = "Beginner",
                    Goal = "Weight Loss"
                },

                new
                {
                    Username = "member4",
                    Email = "member4@fitnessgym.local",
                    Name = "Youssef Samir",
                    Phone = "01000000004",
                    PlanId = monthlyPlan.Id,
                    FitnessLevel = "Intermediate",
                    Goal = "Muscle Gain"
                },

                new
                {
                    Username = "member5",
                    Email = "member5@fitnessgym.local",
                    Name = "Karim Adel",
                    Phone = "01000000005",
                    PlanId = basicPlan.Id,
                    FitnessLevel = "Beginner",
                    Goal = "General Fitness"
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
                        PhoneNumber = data.Phone,
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
                        FitnessLevel = data.FitnessLevel,
                        Goal = data.Goal
                    };

                    context.Members.Add(member);

                    await context.SaveChangesAsync();
                }

                members.Add(member);
            }

            // ==========================================
            // ADDITIONAL MEMBERS
            // ==========================================

            var additionalMembersData = new[]
            {
                new
                {
                    Username = "member6",
                    Email = "member6@fitnessgym.local",
                    Name = "Mostafa Tarek",
                    Phone = "01000000006",
                    PlanId = halfYearPlan.Id,
                    FitnessLevel = "Advanced",
                    Goal = "Bodybuilding"
                },

                new
                {
                    Username = "member7",
                    Email = "member7@fitnessgym.local",
                    Name = "Mahmoud Fathy",
                    Phone = "01000000007",
                    PlanId = annualPlan.Id,
                    FitnessLevel = "Intermediate",
                    Goal = "General Fitness"
                },

                new
                {
                    Username = "member8",
                    Email = "member8@fitnessgym.local",
                    Name = "Amr Nabil",
                    Phone = "01000000008",
                    PlanId = monthlyPlan.Id,
                    FitnessLevel = "Beginner",
                    Goal = "Weight Loss"
                },

                new
                {
                    Username = "member9",
                    Email = "member9@fitnessgym.local",
                    Name = "Tarek Ahmed",
                    Phone = "01000000009",
                    PlanId = quarterlyPlan.Id,
                    FitnessLevel = "Advanced",
                    Goal = "Strength"
                },

                new
                {
                    Username = "member10",
                    Email = "member10@fitnessgym.local",
                    Name = "Khaled Sameh",
                    Phone = "01000000010",
                    PlanId = basicPlan.Id,
                    FitnessLevel = "Beginner",
                    Goal = "Weight Loss"
                },

                new
                {
                    Username = "member11",
                    Email = "member11@fitnessgym.local",
                    Name = "Ali Mahmoud",
                    Phone = "01000000011",
                    PlanId = monthlyPlan.Id,
                    FitnessLevel = "Intermediate",
                    Goal = "Muscle Gain"
                },

                new
                {
                    Username = "member12",
                    Email = "member12@fitnessgym.local",
                    Name = "Seif Ibrahim",
                    Phone = "01000000012",
                    PlanId = annualPlan.Id,
                    FitnessLevel = "Advanced",
                    Goal = "Athletic Performance"
                }
            };

            foreach (var data in additionalMembersData)
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
                        PhoneNumber = data.Phone,
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
                        JoinDate = DateTime.Today.AddDays(-45),
                        MembershipPlanId = data.PlanId,
                        UserId = user.Id,
                        FitnessLevel = data.FitnessLevel,
                        Goal = data.Goal
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

            // Additional subscriptions

            await CreateSubscriptionIfMissing(
                context,
                members[5],
                halfYearPlan,
                DateTime.Today.AddDays(-40),
                DateTime.Today.AddDays(140),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[6],
                annualPlan,
                DateTime.Today.AddDays(-100),
                DateTime.Today.AddDays(265),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[7],
                monthlyPlan,
                DateTime.Today.AddDays(-35),
                DateTime.Today.AddDays(-5),
                "Expired");

            await CreateSubscriptionIfMissing(
                context,
                members[8],
                quarterlyPlan,
                DateTime.Today.AddDays(10),
                DateTime.Today.AddDays(100),
                "Pending");

            await CreateSubscriptionIfMissing(
                context,
                members[9],
                basicPlan,
                DateTime.Today.AddDays(-15),
                DateTime.Today.AddDays(15),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[10],
                monthlyPlan,
                DateTime.Today.AddDays(-2),
                DateTime.Today.AddDays(28),
                "Active");

            await CreateSubscriptionIfMissing(
                context,
                members[11],
                annualPlan,
                DateTime.Today.AddDays(-180),
                DateTime.Today.AddDays(185),
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

                if (!exists &&
                    subscription.MembershipPlan != null)
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

            foreach (var member in members)
            {
                var hasMemberCheckIn =
                    await context.CheckIns
                        .AnyAsync(c =>
                            c.MemberId == member.Id);

                if (!hasMemberCheckIn)
                {
                    context.CheckIns.Add(new CheckIn
                    {
                        MemberId = member.Id,
                        StaffId = staff.Id,
                        CheckInTime =
                            DateTime.Now.AddMinutes(
                                -(member.Id * 12)),
                        Status = member.Id % 4 == 0
                            ? "Rejected-Expired"
                            : "Allowed"
                    });
                }
            }

            await context.SaveChangesAsync();

            // ==========================================
            // CLASSES
            // ==========================================

            var yogaClass = await CreateClassIfMissing(
                context,
                "Yoga",
                15);

            var cardioClass = await CreateClassIfMissing(
                context,
                "Cardio",
                20);

            var strengthClass = await CreateClassIfMissing(
                context,
                "Strength",
                12);

            var crossFitClass = await CreateClassIfMissing(
                context,
                "CrossFit",
                10);

            var bodybuildingClass = await CreateClassIfMissing(
                context,
                "Bodybuilding",
                8);

            var mobilityClass = await CreateClassIfMissing(
                context,
                "Mobility",
                15);

            var absClass = await CreateClassIfMissing(
                context,
                "Abs & Core",
                18);

            // ==========================================
            // GET ALL TRAINERS
            // ==========================================

            var allCoaches = await context.Coaches
                .ToListAsync();

            var coach1 = allCoaches
                .First(c => c.Id == coach.Id);

            var coach2 = additionalCoaches.Count > 0
                ? additionalCoaches[0]
                : coach1;

            var coach3 = additionalCoaches.Count > 1
                ? additionalCoaches[1]
                : coach1;

            var coach4 = additionalCoaches.Count > 2
                ? additionalCoaches[2]
                : coach1;

            var coach5 = additionalCoaches.Count > 3
                ? additionalCoaches[3]
                : coach1;

            // ==========================================
            // CLASS SCHEDULES
            // ==========================================

            await CreateScheduleIfMissing(
                context,
                yogaClass,
                coach1,
                DateTime.Today.AddDays(1).AddHours(10),
                DateTime.Today.AddDays(1).AddHours(11),
                yogaClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                cardioClass,
                coach2,
                DateTime.Today.AddDays(1).AddHours(18),
                DateTime.Today.AddDays(1).AddHours(19),
                cardioClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                strengthClass,
                coach1,
                DateTime.Today.AddDays(2).AddHours(19),
                DateTime.Today.AddDays(2).AddHours(20),
                strengthClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                crossFitClass,
                coach3,
                DateTime.Today.AddDays(2).AddHours(17),
                DateTime.Today.AddDays(2).AddHours(18),
                crossFitClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                bodybuildingClass,
                coach4,
                DateTime.Today.AddDays(3).AddHours(16),
                DateTime.Today.AddDays(3).AddHours(17),
                bodybuildingClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                mobilityClass,
                coach5,
                DateTime.Today.AddDays(3).AddHours(18),
                DateTime.Today.AddDays(3).AddHours(19),
                mobilityClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                absClass,
                coach2,
                DateTime.Today.AddDays(4).AddHours(19),
                DateTime.Today.AddDays(4).AddHours(20),
                absClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                yogaClass,
                coach5,
                DateTime.Today.AddDays(5).AddHours(11),
                DateTime.Today.AddDays(5).AddHours(12),
                yogaClass.Capacity);

            await CreateScheduleIfMissing(
                context,
                strengthClass,
                coach3,
                DateTime.Today.AddDays(6).AddHours(20),
                DateTime.Today.AddDays(6).AddHours(21),
                strengthClass.Capacity);
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

        // ==========================================
        // CLASS HELPER
        // ==========================================

        private static async Task<Class> CreateClassIfMissing(
            ApplicationDbContext context,
            string name,
            int capacity)
        {
            var gymClass = await context.Classes
                .FirstOrDefaultAsync(
                    c => c.Name == name);

            if (gymClass == null)
            {
                gymClass = new Class
                {
                    Name = name,
                    Capacity = capacity
                };

                context.Classes.Add(gymClass);

                await context.SaveChangesAsync();
            }

            return gymClass;
        }

        // ==========================================
        // SCHEDULE HELPER
        // ==========================================

        private static async Task CreateScheduleIfMissing(
            ApplicationDbContext context,
            Class gymClass,
            Coach coach,
            DateTime startTime,
            DateTime endTime,
            int availablePlaces)
        {
            var exists = await context.ClassSchedules
                .AnyAsync(s =>
                    s.ClassId == gymClass.Id &&
                    s.StartTime == startTime);

            if (!exists)
            {
                context.ClassSchedules.Add(new ClassSchedule
                {
                    ClassId = gymClass.Id,
                    CoachId = coach.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                    AvailablePlaces = availablePlaces
                });

                await context.SaveChangesAsync();
            }
        }
    }
}