using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Transactions;

public class WeatherResponse
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature2m { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int Humidity { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed10m { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }
}

public class WeatherWrapper
{
    [JsonPropertyName("current")]
    public WeatherResponse Current { get; set; }
}

class Program
{
    static readonly HttpClient client = new HttpClient();
    static async Task Main()
    {
        Console.WriteLine("GLOBAL WEATHER OBSERVER\n");

        while (true)
        {
            Console.WriteLine("\nEnter city name (or 'exit' to quit):");
            string city = Console.ReadLine();

            if (city.ToLower() == "exit") 
                break;

            Console.Write("Enter Latitude and Longitude (e.g., 40.71 -74.01): ");
            string input = Console.ReadLine();

            string[] parts = input.Split(' ');
            double lat = double.Parse(parts[0]);
            double lon = double.Parse(parts[1]);

            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,apparent_temperature,relative_humidity_2m,wind_speed_10m,weather_code";

                string json = await client.GetStringAsync(url);

                WeatherWrapper wrapper = JsonSerializer.Deserialize<WeatherWrapper>(json);
                WeatherResponse data = wrapper.Current;

                // conversions
                double tempC = data.Temperature2m;
                double tempF = (tempC * 1.8) + 32;

                double feelsLikeC = data.ApparentTemperature;
                double feelsLikeF = (feelsLikeC * 1.8) + 32;

                double windKmh = data.WindSpeed10m;
                double windMph = windKmh * 0.621371;

                // weather conditions
                string condition;

                if (data.WeatherCode == 0)
                {
                    condition = "Clear Sky";
                }
                else if (data.WeatherCode >= 1 && data.WeatherCode <= 3)
                {
                    condition = "Partly Cloudy";
                }
                else if (data.WeatherCode >= 61 && data.WeatherCode <= 65)
                {
                    condition = "Rainy";
                }
                else if (data.WeatherCode >= 71 && data.WeatherCode <= 75)
                {
                    condition = "Snowy";
                }
                else
                {
                    condition = "Cloudy";
                }

                // output
                Console.WriteLine($"--- REPORT FOR {city.ToUpper()} ---");
                Console.WriteLine($"Condition:  {condition}");
                Console.WriteLine($"Actual Temp:  {tempC:F1}°C / {tempF:F1}°F");
                Console.WriteLine($"Feels Like:  {feelsLikeC}°C / {feelsLikeF:F1}°F");
                Console.WriteLine($"Humidity:  {data.Humidity}%");
                Console.WriteLine($"Wind Speed:  {windKmh} km/h / {windMph:F1} mph");

                Console.WriteLine("--------------------------------");

            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        Console.WriteLine("You have read data automatically from a public API!");
    }
}