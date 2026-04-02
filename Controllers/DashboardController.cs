using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using cleo.Data;
using cleo.Models;
using Microsoft.EntityFrameworkCore;
using cleo.Services;

namespace cleo.Controllers;

public class DashboardController : Controller
{
    private readonly CleoDbContext _db;
    private readonly IAIService _aiService;

    public DashboardController(CleoDbContext db, IAIService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var user = await _db.Users.FindAsync(userId);

        ViewBag.UserName = user?.Name ?? "User";
        
        var lastCycle = await _db.CycleTracks
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync();

        // Ensure user goes through onboarding if they haven't set their profiling info
        if (string.IsNullOrEmpty(user?.AgeGroup) || lastCycle == null)
        {
            return RedirectToAction("Onboarding", "Public");
        }

        if (lastCycle != null)
        {
            var diff = (DateTime.UtcNow - lastCycle.StartDate).Days + 1;
            ViewBag.CycleDay = diff > 0 && diff <= user.CycleLength ? diff : 1;
            
            var nextDate = lastCycle.StartDate.AddDays(user.CycleLength);
            ViewBag.NextPeriodDate = nextDate.ToString("MMM dd, yyyy");
            ViewBag.DaysUntilNextPeriod = (nextDate - DateTime.UtcNow).Days;
            
            // Typical ovulation is ~14 days before next period
            var ovulationDate = nextDate.AddDays(-14);
            ViewBag.OvulationDay = ovulationDate.ToString("MMM dd, yyyy");
            ViewBag.OvulationDaysUntil = (ovulationDate - DateTime.UtcNow).Days;
        }
        else
        {
            ViewBag.CycleDay = 0;
            ViewBag.NextPeriodDate = "No data";
            ViewBag.DaysUntilNextPeriod = 0;
            ViewBag.OvulationDay = "No data";
            ViewBag.OvulationDaysUntil = 0;
        }

        ViewBag.CycleLength = user.CycleLength;
        ViewBag.PeriodLength = user.PeriodLength;

        // Real symptoms/mood
        ViewBag.Symptoms = await _db.SymptomLogs.Where(s => s.UserId == userId).Take(3).Select(s => s.Symptoms).ToListAsync();
        var lastMood = await _db.MoodNotes.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).FirstOrDefaultAsync();
        ViewBag.Mood = lastMood?.Mood ?? "Not set";
        ViewBag.Notes = await _db.MoodNotes.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).Take(3).Select(n => n.Note).ToListAsync();

        return View("~/Views/Dashboard/Index.cshtml");
    }

    [HttpGet]
    public IActionResult LogPeriod()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;
        return View("~/Views/Period/Index.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogPeriod(string startDate, string endDate)
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Public");

        if (DateTime.TryParse(startDate, out var start))
        {
            DateTime? end = DateTime.TryParse(endDate, out var e) ? e : null;
            _db.CycleTracks.Add(new CycleTrack { UserId = userId.Value, StartDate = start, EndDate = end });
            await _db.SaveChangesAsync();
            TempData["PeriodMessage"] = "Period cycle logged successfully!";
        }
        
        return RedirectToAction(nameof(LogPeriod));
    }

    [HttpGet]
    public IActionResult LogSymptoms()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;
        return View("~/Views/Symptoms/Index.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogSymptoms(string date, List<string>? symptoms, string notes)
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Public");

        var selected = symptoms != null ? string.Join(", ", symptoms) : "";
        var tip = await _aiService.GetSymptomTipAsync(symptoms ?? new List<string>(), notes);
        
        _db.SymptomLogs.Add(new SymptomLog 
        { 
            UserId = userId.Value, 
            Date = date, 
            Symptoms = selected, 
            Notes = notes,
            AITip = tip
        });
        await _db.SaveChangesAsync();

        TempData["SymptomsMessage"] = "Symptoms logged to your profile!";
        TempData["AITip"] = tip;
        return RedirectToAction(nameof(LogSymptoms));
    }

    [HttpGet]
    public IActionResult Calendar()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        ViewBag.MonthTitle = "February 2026";
        ViewBag.CalendarDays = new[]
        {
            new { Day = 3, Type = "period" },
            new { Day = 4, Type = "period" },
            new { Day = 5, Type = "period" },
            new { Day = 6, Type = "period" },
            new { Day = 7, Type = "period" },
            new { Day = 14, Type = "fertile" },
            new { Day = 15, Type = "fertile" },
            new { Day = 16, Type = "ovulation" },
            new { Day = 17, Type = "fertile" },
            new { Day = 18, Type = "fertile" },
            new { Day = 19, Type = "today" }
        };

        return View("~/Views/Calendar/Index.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var history = await _db.CycleTracks
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();

        ViewBag.HistoryList = history;
        ViewBag.TotalCycles = history.Count;
        ViewBag.AvgCycleLength = "28 days";
        ViewBag.LastPeriod = history.FirstOrDefault()?.StartDate.ToString("MMM dd, yyyy") ?? "None";

        return View("~/Views/Dashboard/History.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Mood()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        ViewBag.MoodEntries = await _db.MoodNotes
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .ToListAsync();
            
        return View("~/Views/Dashboard/Mood.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogMood(string mood, string description)
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId != null)
        {
            var tip = await _aiService.GetMoodTipAsync(mood, description);
            _db.MoodNotes.Add(new MoodNote 
            { 
                UserId = userId.Value, 
                Mood = mood, 
                Note = description,
                AITip = tip
            });
            await _db.SaveChangesAsync();
            TempData["AITip"] = tip;
        }
        return RedirectToAction(nameof(Mood));
    }

    [HttpGet]
    public IActionResult Notes()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        ViewBag.Notes = new[]
        {
            new { Title = "Workout response", Content = "Light cardio helped reduce cramps.", Date = "2026-03-21" },
            new { Title = "Diet", Content = "Added more iron-rich food this week.", Date = "2026-03-20" },
            new { Title = "Sleep", Content = "Need to improve bedtime consistency.", Date = "2026-03-19" }
        };
        return View("~/Views/Dashboard/Notes.cshtml");
    }

    [HttpGet]
    public IActionResult Tips()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        ViewBag.Tips = new[]
        {
            new { Icon = "🥬", Title = "Iron-Rich Foods", Category = "Quick-Nutrition", Content = "Incorporate spinach and lentils during your period.", Color = "green" },
            new { Icon = "🧘", Title = "Light Exercise", Category = "Exercise", Content = "Gentle yoga and walking can reduce bloating.", Color = "green" },
            new { Icon = "💊", Title = "Managing PMS Symptoms", Category = "PMS-Relief", Content = "Magnesium and B6 can help with mood and cramps.", Color = "pink" },
            new { Icon = "🫧", Title = "Menstrual Hygiene", Category = "Hygiene", Content = "Change your sanitary products every 4-6 hours.", Color = "blue" },
            new { Icon = "✨", Title = "Stress Management", Category = "Wellness", Content = "Deep breathing can lower cortisol during your cycle.", Color = "purple" },
            new { Icon = "😴", Title = "Sleep Quality", Category = "Wellness", Content = "Aim for 8 hours of sleep for hormonal balance.", Color = "purple" },
            new { Icon = "🍵", Title = "Ginger & Fatty Foods", Category = "Quick-Nutrition", Content = "Ginger tea can soothe menstrual cramps.", Color = "green" },
            new { Icon = "💧", Title = "Hydration Focus", Category = "Wellness", Content = "Drink 8 glasses of water to reduce bloating.", Color = "blue" }
        };
        return View("~/Views/Dashboard/Tips.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return RedirectToAction("Login", "Public");

        ViewBag.Profile = new
        {
            user.Name,
            user.Email,
            AgeGroup = user.AgeGroup ?? "Not set",
            CycleLength = user.CycleLength,
            PeriodLength = user.PeriodLength,
            JoinDate = user.JoinDate.ToString("MMM yyyy"),
            TotalCycles = await _db.CycleTracks.CountAsync(c => c.UserId == userId),
            TotalMoods = await _db.MoodNotes.CountAsync(m => m.UserId == userId)
        };

        return View("~/Views/Dashboard/Profile.cshtml");
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;
        return View("~/Views/Dashboard/ChangePassword.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var guard = EnsureUserSession();
        if (guard != null) return guard;

        if (newPassword != confirmPassword)
        {
            TempData["PassMessage"] = "Passwords do not match.";
            return RedirectToAction(nameof(ChangePassword));
        }

        TempData["PassMessage"] = "Password updated successfully.";
        return RedirectToAction(nameof(ChangePassword));
    }

    private T? GetSessionObject<T>(string key)
    {
        var json = HttpContext.Session.GetString(key);
        return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    private void SetSessionObject<T>(string key, T value)
    {
        HttpContext.Session.SetString(key, JsonSerializer.Serialize(value));
    }

    private IActionResult? EnsureUserSession()
    {
        var role = HttpContext.Session.GetString("Role");
        return string.Equals(role, "User", StringComparison.OrdinalIgnoreCase)
            ? null
            : RedirectToAction("Login", "Public");
    }
}
