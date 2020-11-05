using Forecasting_BikeSharingDemandLib;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWithML.NET.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeDemandForcastController : ControllerBase
    {

        [HttpGet("GetEvaluateOutput/{numberOfDaysToPredict}")]
        public IActionResult GetEvaluateOutput(int numberOfDaysToPredict)
        {
            var output = BikeForcast.GetBikeForcast(numberOfDaysToPredict);

            return Ok(output.evaluateOutput);
        }


        [HttpGet("GetForecastOutput/{numberOfDaysToPredict}")]
        public IActionResult GetForecastOutput(int numberOfDaysToPredict)
        {
            var output = BikeForcast.GetBikeForcast(numberOfDaysToPredict);

            return Ok(output.forecastOutput);
        }
    }
}
