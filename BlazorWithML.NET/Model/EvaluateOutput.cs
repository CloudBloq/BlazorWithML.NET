using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWithML.NET.Model
{   
    public class EvaluateOutput
    {
        public float MeanAbsoluteError { get; set; }

        public double RootMeanSquaredError { get; set; }
    }
}
