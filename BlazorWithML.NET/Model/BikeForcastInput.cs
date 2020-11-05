using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWithML.NET.Model
{
    public class BikeForcastInput
    {
        [Required(ErrorMessage = "Number of years to predict is required.")]
        [Range(minimum: 1, maximum: 500, ErrorMessage = "Range of values allowed are 1 - 500")]
        public int numberOfDaysToPredict { get; set; }
    }
}
