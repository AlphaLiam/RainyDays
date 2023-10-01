namespace RainyDays
{
    public class Day
    {
        public string Date { get; set; }
        public float? Rainfall { get; set; }

        public Day(string date, float? rain)
        {
            Date = date;
            Rainfall = rain;
        }
    }

    public class Input
    {
        public string Date { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class Output
    {
        public string Date { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public float? TotalRainfall { get; set; }
        public Output (string date, string latitude, string longitude, float? totalRainFall)
        {
            Date = date;
            Latitude = latitude;
            Longitude = longitude;
            TotalRainfall = totalRainFall;
        }
    }



}
