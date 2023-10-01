namespace RainyDays
{
    public class WeatherHistJSON
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public float generationtime_us { get; set; }
        public int utc_offset_seconds { get; set; }
        public string timezone { get; set; }
        public string timezone_abbreviation { get; set; }
        public float elevation { get; set; }
        public DailyUnitsJSON daily_units { get; set; }
        public DailyJSON daily { get; set; }
    }

    public class DailyUnitsJSON
    {
        public string time { get; set; }
        public string rain_sum { get; set; }
    }

    public class DailyJSON
    {
        public List<string> time { get; set; }
        public List<float?> rain_sum { get; set; }
    }
}
