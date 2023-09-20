using ApiWithAuth.Models;

namespace ApiWithAuth.Abstraction
{
    public interface IWeatherClient
    {
        Task<IEnumerable<WeatherForecast>> GetForecastsAsync(string cityName);
    }
}
