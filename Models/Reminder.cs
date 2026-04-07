using System.ComponentModel.DataAnnotations;

namespace cleo.Models;

public class Reminder
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public DateTime ReminderDate { get; set; }
    
    public string Type { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    
    public bool IsEmailSent { get; set; } = false;
    
    public bool IsEnabled { get; set; } = true;
}
