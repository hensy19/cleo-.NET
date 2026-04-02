using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using cleo.Data;
using cleo.Models;
using Microsoft.EntityFrameworkCore;

namespace cleo.Controllers;

public class PublicController : Controller
{
    private readonly CleoDbContext _db;

    public PublicController(CleoDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Home() => View("~/Views/Home/Index.cshtml");

    [HttpGet]
    public IActionResult Login() => View("~/Views/Account/Login.cshtml");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        if (user != null)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", "User");
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Name", user.Name);
            return RedirectToAction("Index", "Dashboard");
        }

        TempData["LoginError"] = "Invalid email or password.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Signup() => View("~/Views/Account/Register.cshtml");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["RegisterMessage"] = "Email is required.";
            return RedirectToAction(nameof(Signup));
        }

        var existingUser = await _db.Users.AnyAsync(u => u.Email == email);
        if (existingUser)
        {
            TempData["RegisterMessage"] = "User with this email already exists.";
            return RedirectToAction(nameof(Signup));
        }

        var newUser = new UserAccount { Name = name ?? "User", Email = email, Password = password ?? "Password123" };
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        HttpContext.Session.SetInt32("UserId", newUser.Id);
        HttpContext.Session.SetString("Role", "User");
        HttpContext.Session.SetString("Email", newUser.Email);
        HttpContext.Session.SetString("Name", newUser.Name);
        
        return RedirectToAction(nameof(Onboarding));
    }

    [HttpGet]
    public IActionResult Onboarding() => View("~/Views/Onboarding/Index.cshtml");

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("onboarding")]
    public async Task<IActionResult> Onboarding(string ageGroup, string lastPeriodDate, int cycleLength)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction(nameof(Login));

        var user = await _db.Users.FindAsync(userId.Value);
        if (user != null)
        {
            user.AgeGroup = ageGroup;
            user.CycleLength = cycleLength > 0 ? cycleLength : 28;
            await _db.SaveChangesAsync();

            if (DateTime.TryParse(lastPeriodDate, out var start))
            {
                // Clear existing predictions to avoid duplicates if onboarding re-run
                var existingCycles = _db.CycleTracks.Where(c => c.UserId == userId.Value);
                _db.CycleTracks.RemoveRange(existingCycles);
                
                _db.CycleTracks.Add(new CycleTrack 
                { 
                    UserId = userId.Value, 
                    StartDate = start 
                });
                await _db.SaveChangesAsync();
            }
        }
        
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View("~/Views/Account/ForgotPassword.cshtml");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View("~/Views/Account/ForgotPasswordConfirmation.cshtml");

    [HttpGet]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("~/Views/Home/NotFound.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Home));
    }
}
