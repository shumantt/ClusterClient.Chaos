using System;
using Microsoft.AspNetCore.Mvc;

namespace ExampleServer.Controllers
{
    [ApiController]
    public class ExampleController : ControllerBase
    {
        [HttpPost("example/measure")]
        public ActionResult<string> MeasureTime([FromBody] DateTime sendTime)
        {
            var time = DateTime.Now - sendTime;
            return Ok($"Received result after around {time}");
        }

        [HttpGet("name")]
        public ActionResult<string> GetName()
        {
            return "Sample Name";
        }
        
        [HttpGet("age")]
        public ActionResult<int> GetAge()
        {
            return 24;
        }

        [HttpGet("city")]
        public ActionResult<string> GetCity()
        {
            return "CityOfChaos";
        }
    }
}