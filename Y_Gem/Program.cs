using Microsoft.EntityFrameworkCore;
using Y_Gem.Data;
using Microsoft.AspNetCore.Identity;
using Y_Gem.Models;
using Y_Gem.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed roles + admin account
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Staff", "Trainer", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }


    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    // var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // var staffEmail = "staff@gym.com";
    // var existingStaffUser = await userManager.FindByEmailAsync(staffEmail);

    // if (existingStaffUser == null)
    // {
    //     var staffUser = new ApplicationUser
    //     {
    //         UserName = staffEmail,
    //         Email = staffEmail,
    //         FullName = "Receptionist (Probationary)",
    //         EmailConfirmed = true
    //     };

    //     var result = await userManager.CreateAsync(staffUser, "Staff@123");
    //     if (result.Succeeded)
    //     {
    //         await userManager.AddToRoleAsync(staffUser, "Staff");

    //         context.Staff.Add(new Staff
    //         {
    //             UserId = staffUser.Id,
    //             JobTitle = "Receptionist",
    //             ShiftTime = "9AM-5PM"
    //         });
    //         await context.SaveChangesAsync();
    //     }



    // }


    // var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    // var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // var coachEmail = "coach@gym.com";
    // if (await userManager.FindByEmailAsync(coachEmail) == null)
    // {
    //     var coach = new Coach
    //     {
    //         UserName = coachEmail,
    //         Email = coachEmail,
    //         FullName = "كابتن أحمد",
    //         EmailConfirmed = true,
    //         coachSpecialty = "Yoga & Zumba"
    //     };

    //     var result = await userManager.CreateAsync(coach, "Coach@123");
    //     if (!result.Succeeded)
    //     {
    //         foreach (var e in result.Errors) Console.WriteLine(e.Description);
    //     }

    // }


}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
