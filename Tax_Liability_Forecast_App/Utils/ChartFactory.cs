using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.ImageSharp;
using OxyPlot.Legends;
using OxyPlot.Series;
using System.IO;

namespace Tax_Liability_Forecast_App.Utils
{
    public class ChartFactory : IChartFactory
    {
        public byte[] CreateIncomeExpensePieChart(Dictionary<string, decimal> data, double availableWidth, double? availableHeight = null)
        {
            double width = availableWidth;
            double height = availableHeight ?? availableWidth * 0.75;

            var model = new PlotModel { Title = "Income vs Expense", Background = OxyColors.White };
            var pieSeries = new PieSeries
            {
                StrokeThickness = 0.25,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0,
            };

            foreach (var keyValue in data)
            {
                var label = keyValue.Key.ToLower();
                var value = (double)keyValue.Value;

                var slice = new PieSlice(keyValue.Key, value)
                {
                    Fill = label.Contains("income") ? OxyColors.LightBlue :
                    label.Contains("expense") ? OxyColors.IndianRed :
                    OxyColors.Gray
                };

                pieSeries.Slices.Add(slice);
            }

            model.Series.Add(pieSeries);
            var stream = new MemoryStream();
            var exporter = new PngExporter((int)width, (int)height, 96);
            exporter.Export(model, stream);

            return stream.ToArray();
        }

        public byte[] CreateTaxOverTimeLineChart(List<(DateTime month, decimal tax, decimal net)> points, double availableWidth, double? availableHeight = null)
        {
            double width = availableWidth;
            double height = availableHeight ?? availableWidth * 2 / 3;

            var model = new PlotModel 
            { 
                Title = "Tax Owed vs Net Income", 
                Background = OxyColors.White,
                IsLegendVisible = true,
            };
            var legend = new Legend
            {
                LegendPosition = LegendPosition.TopRight,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendBorder = OxyColors.Black,
            };
            model.Legends.Add(legend);
            model.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MMM yyyy",
                MinorIntervalType = DateTimeIntervalType.Months,
                IntervalType = DateTimeIntervalType.Months,
            });

            double maxY = points.Max(p => Math.Max((double)p.tax, (double)p.net));
            double yPadding = 1000;

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = 0,
                Maximum = maxY + yPadding,
                LabelFormatter = value => $"{value} €"
            });
            var taxSeries = new LineSeries 
            { 
                Title = "Tax Owed", 
                MarkerType = MarkerType.Circle,
                Color = OxyColors.IndianRed,
            };
            var netSeries = new LineSeries 
            { 
                Title = "Net Income", 
                MarkerType = MarkerType.Square,
                Color = OxyColors.LightBlue,
            };

            foreach (var point in points)
            {
                taxSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.month, (double)point.tax));
                netSeries.Points.Add(DateTimeAxis.CreateDataPoint(point.month, (double)point.net));
            }

            model.Series.Add(taxSeries);
            model.Series.Add(netSeries);

            var stream = new MemoryStream();
            var exporter = new PngExporter((int)width, (int)height, 96);
            exporter.Export(model, stream);

            return stream.ToArray();
        }
    }
}
