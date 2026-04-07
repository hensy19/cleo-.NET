using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cleo.Data;
using cleo.Models;
using cleo.Services;

namespace cleo.Controllers;

public class RemindersController : Controller
{
    private readonly CleoDbContext _db;
    private readonly IReminderService _reminderService;

    public RemindersController(CleoDbContext db, IReminderService reminderService)
    {
        _db = db;
        _reminderService = reminderService;
    }

    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Public");

        var reminders = await _db.Reminders
            .Where(r => r.UserId == userId.Value)
            .OrderBy(r => r.ReminderDate)
            .ToListAsync();

        return View(reminders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, DateTime date, string type)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Public");

        if (!string.IsNullOrEmpty(title))
        {
            var reminder = new Reminder
            {
                UserId = userId.Value,
                Title = title,
                ReminderDate = date,
                Type = type ?? "Custom",
                IsEnabled = true,
                IsEmailSent = false
            };

            _db.Reminders.Add(reminder);
            await _db.SaveChangesAsync();
            TempData["ReminderMsg"] = "Reminder added successfully!";
        }

        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);
        if (reminder != null)
        {
            reminder.IsEnabled = !reminder.IsEnabled;
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);
        if (reminder != null)
        {
            _db.Reminders.Remove(reminder);
            await _db.SaveChangesAsync();
            TempData["ReminderMsg"] = "Reminder deleted.";
        }

        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDate(int id, DateTime newDate)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId.Value);
        if (reminder != null)
        {
            reminder.ReminderDate = newDate;
            reminder.IsEmailSent = false; 
            await _db.SaveChangesAsync();
            TempData["ReminderMsg"] = "Reminder updated.";
        }

        return RedirectToAction(nameof(Manage));
    }
}
