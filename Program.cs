using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using cleo.Data;
using cleo.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<CleoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=cleo.db"));
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<ICyclePredictionService, CyclePredictionService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddHostedService<ReminderBackgroundService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.SaveTokens = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Public/NotFoundPage");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Public", action = "Home" });

app.MapControllerRoute(
    name: "public-login",
    pattern: "login",
    defaults: new { controller = "Public", action = "Login" });

app.MapControllerRoute(
    name: "public-signup",
    pattern: "signup",
    defaults: new { controller = "Public", action = "Signup" });

app.MapControllerRoute(
    name: "onboarding",
    pattern: "onboarding",
    defaults: new { controller = "Public", action = "Onboarding" });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "dashboard",
    defaults: new { controller = "Dashboard", action = "Index" });

app.MapControllerRoute(
    name: "log-period",
    pattern: "log-period",
    defaults: new { controller = "Dashboard", action = "LogPeriod" });

app.MapControllerRoute(
    name: "log-symptoms",
    pattern: "log-symptoms",
    defaults: new { controller = "Dashboard", action = "LogSymptoms" });

app.MapControllerRoute(
    name: "calendar",
    pattern: "calendar",
    defaults: new { controller = "Dashboard", action = "Calendar" });

app.MapControllerRoute(
    name: "history",
    pattern: "history",
    defaults: new { controller = "Dashboard", action = "History" });

app.MapControllerRoute(
    name: "mood",
    pattern: "mood",
    defaults: new { controller = "Dashboard", action = "Mood" });

app.MapControllerRoute(
    name: "notes",
    pattern: "notes",
    defaults: new { controller = "Dashboard", action = "Notes" });

app.MapControllerRoute(
    name: "tips",
    pattern: "tips",
    defaults: new { controller = "Dashboard", action = "Tips" });

app.MapControllerRoute(
    name: "profile",
    pattern: "profile",
    defaults: new { controller = "Dashboard", action = "Profile" });

app.MapControllerRoute(
    name: "admin-login",
    pattern: "admin/login",
    defaults: new { controller = "Admin", action = "Login" });

app.MapControllerRoute(
    name: "admin-dashboard",
    pattern: "admin/dashboard",
    defaults: new { controller = "Admin", action = "Dashboard" });

app.MapControllerRoute(
    name: "admin-users",
    pattern: "admin/users",
    defaults: new { controller = "Admin", action = "Users" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Home}/{id?}");

app.MapControllerRoute(
    name: "fallback",
    pattern: "{*url}",
    defaults: new { controller = "Public", action = "NotFoundPage" });

app.Run();
