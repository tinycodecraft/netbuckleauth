using ApiWithAuth.Abstraction;
using ApiWithAuth.Models;
using CoolWebApi.Extensions;
using System.Net.Http.Headers;

namespace ApiWithAuth.Services
{
    public class WeatherClient : IWeatherClient
    {
        private readonly HttpClient _client;
        private readonly WeatherSetting _settings;

        public WeatherClient(HttpClient client, WeatherSetting settings)
        {
            _client = client;
            _settings = settings;
            _client.BaseAddress = new Uri(_settings.BaseUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IEnumerable<WeatherForecast>> GetForecastsAsync(string cityName)
        {
            var url = $"v1/forecast.json?key={_settings.ApiKey}&q={cityName}&days={_settings.NoDaysForecast}";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var days = JsonSerializerExtensions.DeserializeAnonymousType(content, new
            {
                forecast = new
                {
                    forecastday = new[]
                    {
                       new
                       {
                           date =DateOnly.FromDateTime( DateTime.Today),
                           day = new { avgtemp_c = 0.0, condition = new { text = "" } }
                       }
                    }
                }
            }).forecast.forecastday;

            return days.Select(d => new WeatherForecast
            {
                Date = d.date,
                Summary = d.day.condition.text,
                TemperatureC = (int)d.day.avgtemp_c
            });

            // Other way to deserialize json without creating anonymous object
            // To get more information see https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsonelement?view=net-5.0
            //dynamic result = JsonSerializer.Deserialize<ExpandoObject>(content);
            //var days = result.forecast.GetProperty("forecastday").EnumerateArray();
            //foreach (var day in days)
            //{
            //    var date = day.GetProperty("date").GetDateTime();
            //    var temp = day.GetProperty("day").GetProperty("avgtemp_c").GetDouble();
            //    var condition = day.GetProperty("day").GetProperty("condition").GetProperty("text").GetString();
            //}
        }
    }
}
