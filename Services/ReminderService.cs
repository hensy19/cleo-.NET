using cleo.Data;
using cleo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace cleo.Services;

public class ReminderService : IReminderService
{
    private readonly CleoDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ICyclePredictionService _predictionService;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        CleoDbContext db, 
        IEmailService emailService, 
        ICyclePredictionService predictionService,
        ILogger<ReminderService> logger)
    {
        _db = db;
        _emailService = emailService;
        _predictionService = predictionService;
        _logger = logger;
    }

    public async Task CheckAndGenerateRemindersAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;

        var lastCycle = await _db.CycleTracks
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.StartDate)
            .FirstOrDefaultAsync();

        if (lastCycle == null) return;

        var nextPeriodDate = _predictionService.GetNextPeriodDate(lastCycle.StartDate, user.CycleLength);
        var ovulationDate = _predictionService.GetOvulationDate(lastCycle.StartDate, user.CycleLength);
        var fertileWindow = _predictionService.GetFertileWindow(lastCycle.StartDate, user.CycleLength);

        // Check if reminders for this cycle already exist
        var existingReminders = await _db.Reminders
            .Where(r => r.UserId == userId && r.ReminderDate >= DateTime.UtcNow.Date)
            .ToListAsync();

        if (!existingReminders.Any())
        {
            var remindersToAdd = new List<Reminder>
            {
                new Reminder { UserId = userId, Title = "Period starts tomorrow", ReminderDate = nextPeriodDate.AddDays(-1).Date, Type = "Period" },
                new Reminder { UserId = userId, Title = "Fertile window starts", ReminderDate = fertileWindow.Start.Date, Type = "Fertile" },
                new Reminder { UserId = userId, Title = "Ovulation day is today", ReminderDate = ovulationDate.Date, Type = "Ovulation" }
            };
            _db.Reminders.AddRange(remindersToAdd);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Generated cycle reminders for user {UserId}", userId);
        }
    }

    public async Task ProcessDailyRemindersAsync()
    {
        _logger.LogInformation("Starting daily reminder processing at {Time}", DateTime.Now);
        var today = DateTime.UtcNow.Date;

        // 1. Process standard reminders from database
        var pendingReminders = await _db.Reminders
            .Where(r => r.ReminderDate.Date == today && !r.IsEmailSent && r.IsEnabled)
            .ToListAsync();

        foreach (var reminder in pendingReminders)
        {
            try
            {
                await SendEmailReminderAsync(reminder);
                reminder.IsEmailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending standard reminder {Id}", reminder.Id);
            }
        }

        // 2. Process Special Reminders (Inactivity & Monthly Summary)
        var users = await _db.Users.ToListAsync();
        foreach (var user in users)
        {
            // A) Inactivity Reminder (7 days)
            if (user.LastActivityDate.HasValue && (today - user.LastActivityDate.Value.Date).Days == 7)
            {
                await SendInactivityReminderAsync(user);
            }

            // B) Monthly Summary (1st of month)
            if (today.Day == 1)
            {
                await SendMonthlySummaryAsync(user);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Finished daily reminder processing.");
    }

    public async Task UpdateLastActivityAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastActivityDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> SendEmailReminderAsync(Reminder reminder)
    {
        var user = await _db.Users.FindAsync(reminder.UserId);
        if (user == null || string.IsNullOrEmpty(user.Email)) return "User or email missing";

        string subject = $"Cleo Alert: {reminder.Title}";
        string body = BuildEmailBody(user.Name, reminder.Title, reminder.Type);

        await _emailService.SendEmailAsync(user.Email, subject, body);
        return "Success";
    }

    private async Task SendInactivityReminderAsync(UserAccount user)
    {
        if (string.IsNullOrEmpty(user.Email)) return;

        string subject = "We miss you on Cleo!";
        string body = BuildSpecialEmailBody(user.Name, "Inactivity", "You haven\u2019t updated your health logs in a while.<br/><br/>Come back and keep your cycle predictions accurate.");

        await _emailService.SendEmailAsync(user.Email, subject, body);
        _logger.LogInformation("Sent inactivity reminder to {Email}", user.Email);
    }

    private async Task SendMonthlySummaryAsync(UserAccount user)
    {
        if (string.IsNullOrEmpty(user.Email)) return;

        // Fetch stats for last month
        var lastMonth = DateTime.UtcNow.AddMonths(-1);
        var symptomCount = await _db.SymptomLogs.CountAsync(s => s.UserId == user.Id && DateTime.Parse(s.Date).Month == lastMonth.Month);
        var moodCount = await _db.MoodNotes.CountAsync(m => m.UserId == user.Id && m.Date.Month == lastMonth.Month);

        string content = $@"
            <ul style='list-style: none; padding: 0;'>
                <li>\u2022 Cycle length: {user.CycleLength} days</li>
                <li>\u2022 Symptoms logged: {symptomCount}</li>
                <li>\u2022 Mood logs: {moodCount}</li>
                <li>\u2022 Average flow days: {user.PeriodLength}</li>
            </ul>
            <p>Stay healthy \uD83D\uDDA4</p>";

        string subject = "Your Monthly Health Summary";
        string body = BuildSpecialEmailBody(user.Name, "Monthly Summary", content);

        await _emailService.SendEmailAsync(user.Email, subject, body);
        _logger.LogInformation("Sent monthly summary to {Email}", user.Email);
    }

    private string BuildEmailBody(string userName, string title, string type)
    {
        string color = type.ToLower() switch
        {
            "period" => "#F56565",
            "ovulation" => "#9F7AEA",
            "fertile" => "#9F7AEA",
            _ => "#10517F"
        };

        return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #edf2f7; border-radius: 8px;'>
            <div style='text-align: center; margin-bottom: 20px;'>
                 <h1 style='color: #10517F; margin: 0;'>Cleo</h1>
            </div>
            <div style='background-color: {color}; height: 4px; border-radius: 2px; margin-bottom: 20px;'></div>
            <p>Hi {userName},</p>
            <p style='font-size: 18px; font-weight: bold; color: #10517F;'>{title}</p>
            <p>This is a gentle reminder from your Cleo cycle tracker. Stay prepared and take care of yourself today!</p>
            <hr style='border: 0; border-top: 1px solid #edf2f7; margin: 20px 0;' />
            <p style='font-size: 12px; color: #718096; text-align: center;'>
                &copy; 2026 Cleo App. Stay healthy and empowered.
            </p>
        </div>";
    }

    private string BuildSpecialEmailBody(string userName, string type, string content)
    {
        return $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #edf2f7; border-radius: 8px;'>
            <div style='text-align: center; margin-bottom: 20px;'>
                 <h1 style='color: #10517F; margin: 0;'>Cleo</h1>
                 <p style='color: #718096; font-size: 0.9rem;'>{type}</p>
            </div>
            <div style='background-color: #10517F; height: 4px; border-radius: 2px; margin-bottom: 20px;'></div>
            <p>Hi {userName},</p>
            <div style='font-size: 16px; color: #2D3748; line-height: 1.6;'>
                {content}
            </div>
            <hr style='border: 0; border-top: 1px solid #edf2f7; margin: 20px 0;' />
            <p style='font-size: 12px; color: #718096; text-align: center;'>
                \u24B8 2026 Cleo App. Log your health daily for better predictions.
            </p>
        </div>";
    }
}
