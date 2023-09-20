using ApiWithAuth.Resources;
using System.ComponentModel.DataAnnotations;

namespace ApiWithAuth.Models;

/// <summary>
/// Weather Forecast From weather api
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Date on which the Weather Reported
    /// 
    /// </summary>
    [Display(ResourceType = typeof(DisplayNameResource), Name = "WeatherDate")]
    [Required(ErrorMessageResourceType = typeof(ErrorMessageResource), ErrorMessageResourceName = "RequiredError")]
    public DateOnly Date { get; set; }
    /// <summary>
    /// The Temperature in Degree Celsius according to Weather Reported
    /// </summary>
    [Display(ResourceType = typeof(DisplayNameResource), Name = "TemperatureC")]
    [Required(ErrorMessageResourceType = typeof(ErrorMessageResource), ErrorMessageResourceName = "RequiredError")]
    public int TemperatureC { get; set; }
    /// <summary>
    /// The Temperature in Degree Fahrenheit according to Weather Reported (auto)
    /// </summary>
    [Display(ResourceType = typeof(DisplayNameResource), Name = "TemperatureF")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    /// <summary>
    /// The summary of the Weather Report
    /// </summary>
    public string? Summary { get; set; }
}