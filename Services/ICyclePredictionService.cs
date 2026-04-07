namespace cleo.Services;

public interface ICyclePredictionService
{
    DateTime GetNextPeriodDate(DateTime lastPeriodDate, int cycleLength);
    DateTime GetOvulationDate(DateTime lastPeriodDate, int cycleLength);
    (DateTime Start, DateTime End) GetFertileWindow(DateTime lastPeriodDate, int cycleLength);
    string GetCyclePhase(DateTime lastPeriodDate, int cycleLength, int periodLength);
    int GetDaysLeft(DateTime targetDate);
    int GetCurrentCycleDay(DateTime lastPeriodDate);
}
