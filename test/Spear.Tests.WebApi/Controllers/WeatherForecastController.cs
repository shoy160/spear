using Microsoft.AspNetCore.Mvc;
using Spear.Core;
using Spear.Core.Dependency;
using Spear.Tests.WebApi.Domain;
using Spear.WebApi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Spear.Tests.WebApi.Controllers
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherForecastController : DController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly UserRepository _repository;

        public WeatherForecastController(UserRepository repository)
        {
            //_logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var rng = new Random();
            var weathers = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray();
            //return Json(weathers);
            return new JsonResult(CurrentIocManager.ContextWrap.RemoteIpAddress);
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
