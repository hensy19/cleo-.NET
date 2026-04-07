namespace cleo.Services;

public class CyclePredictionService : ICyclePredictionService
{
    public DateTime GetNextPeriodDate(DateTime lastPeriodDate, int cycleLength)
    {
        return lastPeriodDate.AddDays(cycleLength);
    }

    public DateTime GetOvulationDate(DateTime lastPeriodDate, int cycleLength)
    {
        // Typical ovulation is ~14 days before next period
        var nextPeriod = GetNextPeriodDate(lastPeriodDate, cycleLength);
        return nextPeriod.AddDays(-14);
    }

    public (DateTime Start, DateTime End) GetFertileWindow(DateTime lastPeriodDate, int cycleLength)
    {
        var ovulation = GetOvulationDate(lastPeriodDate, cycleLength);
        // Fertile window is typically 5 days leading up to ovulation and the day of ovulation (or +1 day)
        return (ovulation.AddDays(-5), ovulation.AddDays(1));
    }

    public string GetCyclePhase(DateTime lastPeriodDate, int cycleLength, int periodLength)
    {
        int cycleDay = GetCurrentCycleDay(lastPeriodDate);

        if (cycleDay <= periodLength)
            return "Menstrual Phase";
        
        var ovulationDate = GetOvulationDate(lastPeriodDate, cycleLength);
        int ovulationDay = (ovulationDate - lastPeriodDate).Days + 1;

        if (cycleDay < ovulationDay - 5)
            return "Follicular Phase";
        
        if (cycleDay >= ovulationDay - 5 && cycleDay <= ovulationDay + 1)
            return "Ovulation (Fertile Window)";
        
        return "Luteal Phase";
    }

    public int GetDaysLeft(DateTime targetDate)
    {
        var diff = (targetDate - DateTime.UtcNow).Days;
        return diff; // Let caller decide if they want to floor at 0
    }

    public int GetCurrentCycleDay(DateTime lastPeriodDate)
    {
        var diff = (DateTime.UtcNow - lastPeriodDate).Days + 1;
        return diff > 0 ? diff : 1;
    }
}
