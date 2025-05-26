namespace Shared.Data
{
    public enum DayOfTheWeek
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }
    public static class DayOfTheWeekExtensions
    {
        public static DayOfTheWeek ToCustomDayOfWeek(this DateTime dateTime)
        {
            return (DayOfTheWeek)(((int)dateTime.DayOfWeek + 6) % 7 + 1);
        }
    }
}
