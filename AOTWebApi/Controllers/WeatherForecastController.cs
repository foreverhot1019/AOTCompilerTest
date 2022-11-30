using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace AOTWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _logger.LogInformation("WeatherForecastController----------------init");
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Get-----WeatherForecastController----------------");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task<IEnumerable<WeatherForecast>> Post()
        {
            var arr = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
            //return await Task.FromResult(arr);
            return arr;
        }
    }

    /// <summary>
    /// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
    /// AOT发布不是预期的 WeatherForecastp[]
    /// 而是 result
    /// </summary>
    [JsonSerializable(typeof(IEnumerable<WeatherForecast>))]
    [JsonSerializable(typeof(DateOnly))] 
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(System.DayOfWeek))]
    [JsonSerializable(typeof(System.Object))]
    internal partial class WeatherForecastJsonContext : JsonSerializerContext
    {
        public JsonTypeInfo<IEnumerable<WeatherForecast>> WeatherForecasts { get; }
    }
}