namespace ApiWithAuth.Models
{
    public class WeatherSetting
    {
        public string ApiKey { get; set; }

        public string BaseUrl { get; set; }

        public int NoDaysForecast { get; set; }
    }
}
