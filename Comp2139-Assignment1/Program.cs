using Comp2139_Assignment1.Data;
using Comp2139_Assignment1.Services;
using Comp2139_Assignment1.Areas.InventoryManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add the context to the service collection with a connection string
builder.Services.AddDbContext<InventoryDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Identity services for ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true; // Set true if using email confirmation
    })
    .AddEntityFrameworkStores<InventoryDBContext>()
    .AddDefaultTokenProviders();

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

// Inject out MailTrap email sender
builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
try
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDBContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roles and super admin user
    await ContextSeed.SeedRolesAsync(userManager, roleManager);
    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = loggerFactory.CreateLogger("Program");
    logger.LogError(ex, "An error occurred while seeding roles to the DB.");
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Global error handling for production environment
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
}
else
{
    // Detailed exception page in development environment
    app.UseDeveloperExceptionPage();
}
app.UseStatusCodePagesWithRedirects("/Home/CustomNotFound?statusCode={0}");

app.MapRazorPages();
app.MapStaticAssets();

// Map controllers for areas and default routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Project}/{action=Index}/{id?}"
);

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
