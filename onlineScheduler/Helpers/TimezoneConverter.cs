using GeoTimeZone;
using TimeZoneConverter;

namespace CompanyService.Helpers
{
    public static class TimezoneConverter
    {
        public static TimeZoneInfo GetTimezoneFromLocation(double longt, double lat)
        {
            string tzIana = TimeZoneLookup.GetTimeZone(lat, longt).Result;
            TimeZoneInfo tzInfo = TZConvert.GetTimeZoneInfo(tzIana);
            return tzInfo;
        }
    }
}
