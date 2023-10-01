using System.Text.Json;
using CsvHelper;

namespace RainyDays
{
    public class Program
    {
        static readonly HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            Console.WriteLine("Hey Maddy <3");

            bool newMethod = true;
            if (args.Length == 0)
            {
                newMethod = AskYN("Do you have a CSV file to read? (y/n): ");
            }

            if (!newMethod)
            {
                OldMethod();
            } else
            {
                NewMethod(args);
            }

            Console.WriteLine("Bye Maddy, I love you!");
        }

        // Takes a CSV file to gather data
        private static void NewMethod(string[] args)
        {
            bool repeat = true;
            while (repeat)
            {
                string filepath;
                if (args.Length > 0)
                {
                    filepath = args[0];
                } else
                {
                    Console.Write("Enter the filepath to your CSV file: ");
                    filepath = Console.ReadLine();
                }
                
                try
                {
                    using (StreamReader streamReader = new StreamReader(filepath))
                    using (CsvReader csvReader = new CsvReader(streamReader, System.Globalization.CultureInfo.CurrentCulture))
                    {
                        Input single = new Input();
                        IEnumerable<Input> inputData = csvReader.EnumerateRecords(single);
                        List<Output> outputData = new List<Output>();
                        foreach (Input input in inputData)
                        {
                            string json = requestData(input.Date, input.Latitude, input.Longitude);
                            WeatherHistJSON? hist = JsonSerializer.Deserialize<WeatherHistJSON>(json);
                            if (hist == null)
                            {
                                outputData.Add(new Output(input.Date, input.Latitude, input.Longitude, null));
                            }
                            else
                            {
                                // Get total amount of rain for week
                                float totalRain = 0.0f;
                                for (int i = 0; i < hist.daily.rain_sum.Count; i++)
                                {
                                    if (hist.daily.rain_sum[i] != null)
                                    {
                                        totalRain += (float)hist.daily.rain_sum[i];
                                    }
                                }

                                outputData.Add(new Output(input.Date, input.Latitude, input.Longitude, totalRain));
                            }
                        }

                        // Save output to new CSV file
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/RainyDaysOutput");
                        string path = Directory.GetCurrentDirectory() + "/RainyDaysOutput/" + outputData[0].Date + ".csv";
                        using (StreamWriter writer = new StreamWriter(path))
                        using (CsvWriter csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
                        {
                            csv.WriteRecords(outputData);
                        }
                        Console.WriteLine("CSV saved to {0}", path);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("This is an incorrect filepath.");
                }
                catch (CsvHelperException)
                {
                    Console.WriteLine("This file was not properly formatted.\n" +
                                      "The CSV data should have the following columns: Date, Latitude, Longitude");
                }

                if (args.Length > 0)
                {
                    repeat = false;
                } else
                {
                    repeat = AskYN("Would you like to enter another CSV? (y/n): ");
                }
            }
        }

        private static bool AskYN(string question)
        {
            string input = "";
            while (input != "y" && input != "n")
            {
                Console.Write(question);
                input = Console.ReadLine().ToLower();
            }

            if (input == "n")
            {
                return false;
            }
            return true;
        }

        // Read web page contents
        private async static Task<string> ReadWebPage(string url)
        {
            try
            {
                return await client.GetStringAsync(url);
            } catch
            {
                return null;
            }
        }

        private static String requestData(string date, string latitude, string longitude)
        {
            string timezone = "America%2FLos_Angeles";
            DateTime endDate;
            bool valid = DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out endDate);
            if (!valid)
            {
                return "error";
            }
            DateTime startDate = endDate.AddDays(-6);

            // Build API url
            string url = "https://archive-api.open-meteo.com/v1/archive?";
            url += string.Format("latitude={0}", latitude);
            url += string.Format("&longitude={0}", longitude);
            url += "&start_date=" + startDate.ToString("yyyy-MM-dd");
            url += "&end_date=" + endDate.ToString("yyyy-MM-dd");
            url += "&daily=rain_sum";
            url += "&timezone=" + timezone;
            url += "&precipitation_unit=inch";
            
            // Retrieve results from API
            Task<string> t = ReadWebPage(url);
            if (t == null)
            {
                return "error";
            }
            string JSONString = ReadWebPage(url).Result;
            return JSONString;
        }

        // The old, more simplistic, functionality of the program
        // Doesn't take in a CSV file; instead asks for user to input days and coords etc.
        private static void OldMethod()
        {
            float[] coords = { 38.51f, -122.81f };
            GetCoords(coords);

            bool repeat = true;
            // Program repeats as long as user wants to give more dates
            while (repeat)
            {
                DateTime endDate = GetDate();
                int numDays = GetNumDays();
                DateTime startDate = endDate.AddDays(-numDays);
                string timezone = "America%2FLos_Angeles";

                // Build API url
                string url = "https://archive-api.open-meteo.com/v1/archive?";
                url += string.Format("latitude={0}", coords[0]);
                url += string.Format("&longitude={0}", coords[1]);
                url += "&start_date=" + startDate.ToString("yyyy-MM-dd");
                url += "&end_date=" + endDate.ToString("yyyy-MM-dd");
                url += "&daily=rain_sum";
                url += "&timezone=" + timezone;
                url += "&precipitation_unit=inch";

                // Retrieve results from API
                Task<string> t = ReadWebPage(url);
                if (t == null)
                {
                    Console.WriteLine("Could not get data from source");
                    return;
                }
                string JSONString = ReadWebPage(url).Result;

                // Turn API results into JSON object
                var hist = JsonSerializer.Deserialize<WeatherHistJSON>(JSONString);
                if (hist == null)
                {
                    Console.WriteLine("Was unable to deserialize JSON");
                    return;
                }

                PrintData(hist);
                SaveCSV_Old(hist);
                Console.WriteLine("CSV saved to {0}", Directory.GetCurrentDirectory() + "\\RainyDaysOutput\\" + endDate.ToString("yyyy-MM-dd") + ".csv");
                Console.WriteLine("");

                // Check if user wants to give another date
                string input = "";
                while (input != "y" && input != "n")
                {
                    Console.Write("Would you like to give another date? (y/n): ");
                    input = Console.ReadLine().ToLower();
                }
                if (input == "n")
                {
                    repeat = false;
                }
            }
        }

        // Input date in yyyy-mm-dd format
        private static DateTime GetDate()
        {
            DateTime endDate = DateTime.Now;
            bool validDate = false;

            Console.Write("Enter a date (yyyy-mm-dd): ");
            // Check ask user for date until user gives valid date
            while (!validDate)
            {
                string inputDate = Console.ReadLine();
                validDate = DateTime.TryParseExact(inputDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out endDate);
                if (!validDate)
                {
                    Console.Write("Invalid date (yyyy-mm-dd): ");
                }
            }

            return endDate;
        }

        // Input the number of days to look back to
        private static int GetNumDays()
        {
            int numDays = -1;
            bool validInt = false;

            Console.Write("Enter the number of days to go back: ");
            while (!validInt)
            {
                string inputInt = Console.ReadLine();
                validInt = Int32.TryParse(inputInt, System.Globalization.NumberStyles.Integer, null, out numDays);
                if (!validInt)
                {
                    Console.Write("Not a valid number: ");
                }
                else if (numDays < 0)
                {
                    validInt = false;
                    Console.Write("Number can't be negative: ");
                }
            }

            return numDays;
        }

        // Input the coordinates to look for rain
        private static float[] GetCoords(float[] coords)
        {
            Console.WriteLine("The default coordinates are {0}, {1}", coords[0], coords[1]);
            string input = "";
            while (input != "y" && input != "n")
            {
                Console.Write("Would you like to change coordinates? (y/n): ");
                input = Console.ReadLine().ToLower();
            }

            if (input == "n")
            {
                return coords;
            }

            bool validFloat = false;
            Console.Write("Enter a latitude: ");
            while (!validFloat)
            {
                input = Console.ReadLine();
                validFloat = float.TryParse(input, System.Globalization.NumberStyles.Float, null, out coords[0]);
                if (!validFloat)
                {
                    Console.Write("Not a valid number: ");
                }
                else if (coords[0] > 90 || coords[0] < -90)
                {
                    validFloat = false;
                    Console.Write("Latitude ranges from -90 to 90 degrees: ");
                }
            }

            validFloat = false;
            Console.Write("Enter a longitude: ");
            while (!validFloat)
            {
                input = Console.ReadLine();
                validFloat = float.TryParse(input, System.Globalization.NumberStyles.Float, null, out coords[1]);
                if (!validFloat)
                {
                    Console.Write("Not a valid number: ");
                }
                else if (coords[1] > 180 || coords[1] < -180)
                {
                    validFloat = false;
                    Console.Write("Longitude ranges from -180 to 180 degrees: ");
                }
            }

            return coords;
        }

        // Print JSON data to console
        private static void PrintData(WeatherHistJSON hist)
        {
            Console.WriteLine("");
            Console.WriteLine("Date        | Rain (inches)");
            float sum = 0;
            for (int i = 0; i < hist.daily.time.Count; i++)
            {
                Console.Write("{0}  | ", hist.daily.time[i]);
                if (hist.daily.rain_sum[i] == null)
                {
                    Console.WriteLine("NaN");
                }
                else
                {
                    Console.WriteLine(hist.daily.rain_sum[i]);
                    sum += (float)hist.daily.rain_sum[i];
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Total of {0} inches of rainfall over {1} days.", sum, hist.daily.time.Count);
        }

        // Save JSON data to CSV file
        private static void SaveCSV_Old(WeatherHistJSON hist)
        {
            List<Day> days = new List<Day>();
            for (int i = 0; i < hist.daily.time.Count; i++)
            {
                if (hist.daily.rain_sum[i] == null)
                {
                    days.Add(new Day(hist.daily.time[i], Single.NaN));
                }
                else
                {
                    days.Add(new Day(hist.daily.time[i], hist.daily.rain_sum[i]));
                }
            }

            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/RainyDaysOutput");
            string path = Directory.GetCurrentDirectory() + "/RainyDaysOutput/" + days[days.Count - 1].Date + ".csv";
            using (StreamWriter writer = new StreamWriter(path))
            using (CsvWriter csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture))
            {
                csv.WriteRecords(days);
            }
        }
    }
}