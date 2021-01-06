using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ExampleServer.Controllers
{
    [ApiController]
    public class ExampleController : ControllerBase
    {
        [HttpGet("name")]
        public ActionResult<string> GetName() => "Sample Name";

        [HttpGet("age")]
        public ActionResult<int> GetAge() => 24;

        [HttpGet("city")]
        public ActionResult<string> GetCity() => "CityOfChaos";
        

        [HttpGet("error")]
        public ActionResult FailureEndpoint(bool success)
        {
            if (success)
            {
                return Ok();
            }
            
            throw new Exception("Oooops");
        }

        [HttpPost("example/measure")]
        public ActionResult<string> MeasureTime([FromBody] DateTime sendTime, bool success)
        {
            var time = DateTime.Now - sendTime;
            var message = $"Received result after around {time}";
            if (success)
            {
                return Ok(message);
            }

            return new ObjectResult(message)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}