using GeoTimeZone;
using TimeZoneConverter;

namespace CompanyService.Helpers
{
    public static class TimezoneConverter
    {
        public static TimeZoneInfo GetTimezoneFromLocation(double longitude, double latitude)
        {
            string tzIana = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;
            TimeZoneInfo tzInfo = TZConvert.GetTimeZoneInfo(tzIana);
            return tzInfo;
        }

        public static DateTime GetUTCTimeByLocation(DateTime startDateLOC, double latitude, double longitude)
        {
            var tzInfo = GetTimezoneFromLocation(longitude, latitude);
            return TimeZoneInfo.ConvertTimeToUtc(startDateLOC, tzInfo);
        }
    }
}
