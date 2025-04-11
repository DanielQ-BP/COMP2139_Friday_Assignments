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
        options.SignIn.RequireConfirmedAccount = false; // Set true if using email confirmation
        
        
    })
    .AddEntityFrameworkStores<InventoryDBContext>()
    .AddDefaultTokenProviders();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Inject out MailTrap email sender
builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");
}

using var scope = app.Services.CreateScope();
var loggerfactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
try
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDBContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await ContextSeed.SeedRolesAsync(userManager, roleManager);
    await ContextSeed.SeedSuperAdminAsync(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = loggerfactory.CreateLogger("Program");
    logger.LogError(ex, "An error occurred seeding roles to the DB.");
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Project}/{action=Index}/{id?}"
);

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();