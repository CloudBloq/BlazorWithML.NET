using Forecasting_BikeSharingDemandLib.Model;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Forecasting_BikeSharingDemandLib
{
    public class BikeForcast
    {
        private static (string connectionString, string modelPath) GetConnectionString()
        {
            string rootDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../"));
            string dbFilePath = Path.Combine(rootDir, "Data", "BikeDailyDemand.mdf");
            string modelPath = Path.Combine(rootDir, "MLModel.zip");
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30;";
            return (connectionString, modelPath);
        }

        public static (EvaluateOutput evaluateOutput, List<ForecastOutput> forecastOutput) GetBikeForcast(int numberOfDaysToPredict)
        {
            MLContext mlContext = new MLContext();

            //Create DatabaseLoader that loads records of type ModelInput.           
            DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ModelInput>();

            //Define the query to load the data from the database.
            string query = "SELECT RentalDate, CAST(Year as REAL) as Year, CAST(TotalRentals as REAL) as TotalRentals FROM Rentals";

            //Connect to the database and execute the query.
            DatabaseSource dbSource = new DatabaseSource(SqlClientFactory.Instance,
                                            GetConnectionString().connectionString,
                                            query);

            //Load the data into an IDataView.
            IDataView dataView = loader.Load(dbSource);

            //Filter the data
            IDataView firstYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", upperBound: 1);
            IDataView secondYearData = mlContext.Data.FilterRowsByColumn(dataView, "Year", lowerBound: 1);

            //Define time series analysis pipeline
            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedRentals",
                inputColumnName: "TotalRentals",
                windowSize: 7,
                seriesLength: 30,
                trainSize: 365,
                horizon: numberOfDaysToPredict,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundRentals",
                confidenceUpperBoundColumn: "UpperBoundRentals");

            //Use the Fit method to train the model and fit the data to the previously defined forecastingPipeline.
            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(firstYearData);

            //Evaluate the model
            EvaluateOutput evaluateOutput = Evaluate(secondYearData, forecaster, mlContext);

            //Save the model
            var forecastEngine = forecaster.CreateTimeSeriesEngine<ModelInput, ModelOutput>(mlContext);
            forecastEngine.CheckPoint(mlContext, GetConnectionString().modelPath);

            //Use the model to forecast demand
            List<ForecastOutput> forecastOutput = Forecast(secondYearData, numberOfDaysToPredict, forecastEngine, mlContext);

            return (evaluateOutput, forecastOutput);
        }

        private static EvaluateOutput Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            // Make predictions
            IDataView predictions = model.Transform(testData);

            // Actual values
            IEnumerable<float> actual =
                mlContext.Data.CreateEnumerable<ModelInput>(testData, true)
                    .Select(observed => observed.TotalRentals);

            // Predicted values
            IEnumerable<float> forecast =
                mlContext.Data.CreateEnumerable<ModelOutput>(predictions, true)
                    .Select(prediction => prediction.ForecastedRentals[0]);

            // Calculate error (actual - forecast)
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);

            // Get metric averages
            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error

            // Output metrics            
            var evaluateOutput = new EvaluateOutput
            {
                MeanAbsoluteError = MAE,
                RootMeanSquaredError = RMSE
            };

            return evaluateOutput;
        }

        private static List<ForecastOutput> Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ModelInput, ModelOutput> forecaster, MLContext mlContext)
        {
            List<ForecastOutput> forecastOutputList = new List<ForecastOutput>();

            //use the Predict method to forecast rentals.
            ModelOutput forecast = forecaster.Predict();

            IEnumerable<ForecastOutput> forecastOutput =
                mlContext.Data.CreateEnumerable<ModelInput>(testData, reuseRowObject: false)
                    .Take(horizon)
                    .Select((ModelInput rental, int index) =>
                    {
                        string rentalDate = rental.RentalDate.ToShortDateString();
                        float actualRentals = rental.TotalRentals;
                        float lowerEstimate = Math.Max(0, forecast.LowerBoundRentals[index]);
                        float estimate = forecast.ForecastedRentals[index];
                        float upperEstimate = forecast.UpperBoundRentals[index];
                        return new ForecastOutput
                        {
                            Date = rentalDate,
                            ActualRentals = actualRentals,
                            LowerEstimate = lowerEstimate,
                            Forecast = estimate,
                            UpperEstimate = upperEstimate
                        };
                    });

            // Output predictions          
            foreach (var prediction in forecastOutput)
            {
                forecastOutputList.Add(prediction);
            }

            return forecastOutputList;
        }

    }

    public class ModelInput
    {
        public DateTime RentalDate { get; set; }

        public float Year { get; set; }

        public float TotalRentals { get; set; }
    }

    public class ModelOutput
    {
        public float[] ForecastedRentals { get; set; }

        public float[] LowerBoundRentals { get; set; }

        public float[] UpperBoundRentals { get; set; }
    }
}
