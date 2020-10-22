using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Forecasting_BikeSharingDemandLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWithML.NET.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeDemandForcastController : ControllerBase
    {

        [HttpGet("GetEvaluateOutput/{numberOfYearsToPredict}")]
        public IActionResult GetEvaluateOutput(int numberOfYearsToPredict)
        {
            var output = BikeForcast.GetBikeForcast(numberOfYearsToPredict);

            return Ok(output.evaluateOutput);
        }

        
        [HttpGet("GetForecastOutput/{numberOfYearsToPredict}")]
        public IActionResult GetForecastOutput(int numberOfYearsToPredict)
        {
            var output = BikeForcast.GetBikeForcast(numberOfYearsToPredict);

            return Ok(output.forecastOutput);
        }
    }
}
