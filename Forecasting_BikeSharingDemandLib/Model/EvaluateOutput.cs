using System;
using System.Collections.Generic;
using System.Text;

namespace Forecasting_BikeSharingDemandLib.Model
{
    public class EvaluateOutput
    {
        public float MeanAbsoluteError { get; set; }

        public double RootMeanSquaredError { get; set; }
    }
}
