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
            }).ToArray();
        }

        [Route("GetWF")]
        [HttpGet]
        public WeatherForecast GetWF()
        {
            _logger.LogInformation("Get-----WeatherForecastController----------------");
            return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(1,10))),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }

        [Route("GetWFTask")]
        [HttpGet]
        public async Task<WeatherForecast> GetWFTask()
        {
            _logger.LogInformation("Get-----WeatherForecastController----------------");
            return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(Random.Shared.Next(1, 10))),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
        }

        [HttpPost]
        public async Task<List<WeatherForecast>> Post()
        {
            var arr = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();
            //return await Task.FromResult(arr);
            return arr;
        }

    }

    /// <summary>
    /// https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/
    /// AOT发布不是预期的 WeatherForecastp[]
    /// 而是 result
    /// </summary>
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(WeatherForecast[]), GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(List<WeatherForecast>), GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(WeatherForecast), GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(System.Object))] 
    [JsonSerializable(typeof(System.DayOfWeek))]
    internal partial class WeatherForecastJsonContext : JsonSerializerContext
    {
        //public JsonTypeInfo<IEnumerable<WeatherForecast>> WeatherForecasts { get; }
    }
}