using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Y_GYM.Data;
using Y_GYM.Models;
using Y_GYM.Repository;
using static Y_GYM.Repository.CheckInRepository;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// Localization
// ============================================================

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});


// ============================================================
// Database
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ============================================================
// Identity
// ============================================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;

    options.SignIn.RequireConfirmedAccount = false;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// ============================================================
// Authentication Cookie Configuration
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.SlidingExpiration = true;

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});


// ============================================================
// Repositories
// ============================================================

// Members
builder.Services.AddScoped<IMemberRepository, MemberRepository>();

// Staff
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// Coaches / Trainers
builder.Services.AddScoped<ICoachRepository, CoachRepository>();

// Classes
builder.Services.AddScoped<IClassRepository, ClassRepository>();

// Class Schedules
builder.Services.AddScoped<IClassScheduleRepository, ClassScheduleRepository>();

// Bookings
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Check-ins
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();

// Diet Plans
builder.Services.AddScoped<IDietPlanRepository, DietPlanRepository>();

// Payments
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Progress
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();

// Subscriptions
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

// Membership Plans
builder.Services.AddScoped<IMembershipPlans, MembershipPlans>();

// Application Users
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();

// Admin
// Kept temporarily for the current project structure.
// Admin management will later use ApplicationUser + IdentityRole.
builder.Services.AddScoped<IAdminRepository, AdminRepository>();


// ============================================================
// MVC + Localization
// ============================================================

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();


// ============================================================
// Build Application
// ============================================================

var app = builder.Build();


// ============================================================
// Supported Languages
// ============================================================

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),

    SupportedCultures = supportedCultures,

    SupportedUICultures = supportedCultures
};


// ============================================================
// Database Seeder
// ============================================================
//
// DatabaseSeeder is now responsible for:
// 
// - Database migration
// - Roles
// - Default Admin
// - Test Staff
// - Test Members
// - Membership Plans
// - Subscriptions
// - Payments
// - Check-Ins
//
// The Seeder is designed to avoid duplicating the test data
// every time the application starts.
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context =
        services.GetRequiredService<ApplicationDbContext>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    await DatabaseSeeder.SeedAsync(
        context,
        userManager,
        roleManager);
}


// ============================================================
// HTTP Request Pipeline
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// Localization Middleware
// ============================================================

app.UseRequestLocalization(localizationOptions);


// ============================================================
// Routing
// ============================================================

app.UseRouting();


// ============================================================
// Authentication
// ============================================================

app.UseAuthentication();


// ============================================================
// Authorization
// ============================================================

app.UseAuthorization();


// ============================================================
// Static Files
// ============================================================

app.MapStaticAssets();


// ============================================================
// Default MVC Route
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// ============================================================
// Run Application
// ============================================================

app.Run();