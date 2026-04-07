using cleo.Models;

namespace cleo.Services;

public interface IReminderService
{
    Task CheckAndGenerateRemindersAsync(int userId);
    Task ProcessDailyRemindersAsync();
    Task UpdateLastActivityAsync(int userId);
    Task<string> SendEmailReminderAsync(Reminder reminder);
}
