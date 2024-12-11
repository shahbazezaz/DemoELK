using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace DemoELK.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger _logger;

        public WeatherForecastController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            try
            {
                _logger.Information("Detailed log message with context: {CorrelationId}", Guid.NewGuid());
                //throw new ArgumentNullException();
                var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
            .ToArray();
                _logger.Information("Generated {ForecastCount} weather forecasts", forecast.Length);
                return Ok(forecast);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,"error occurred");
                return new StatusCodeResult(500);
            }

        }
    }
}
