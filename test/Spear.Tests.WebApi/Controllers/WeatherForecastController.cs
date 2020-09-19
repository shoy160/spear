using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Spear.Core;
using Spear.Core.Timing;
using Spear.Tests.WebApi.Domain;
using Spear.WebApi;

namespace Spear.Tests.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : DController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        //private readonly ILogger<WeatherForecastController> _logger;

        private readonly UserRepository _repository;

        public WeatherForecastController(UserRepository repository)
        {
            //_logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public async Task<DResult> Create()
        {
            //var result = await _repository.InsertAsync(new Domain.Entities.TUser
            //{
            //    Nick = "shay",
            //    Role = 2,
            //    CreateTime = Clock.Now
            //});
            var result = await _repository.QueryByIdAsync(1001);
            return Succ(result);
        }
    }
}
