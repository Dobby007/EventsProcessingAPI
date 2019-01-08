using EventsChart.ChartData;
using EventsChart.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventsChart
{
    internal class Visualizer
    {
        private readonly IChartArea _chartArea;
        private Brush _lineBrush;
        private Path _path;

        public Visualizer(IChartArea chartArea)
        {
            _chartArea = chartArea;
            _lineBrush = new SolidColorBrush(Colors.Coral);
            _lineBrush.Freeze();
        }

        public async Task Redraw()
        {
            var container = _chartArea.BucketContainer;
            if (container == null)
                return;

            
            
            var stopwatch = Stopwatch.StartNew();
            long start = container.FirstTimestamp + _chartArea.Offset;
            long segmentSize = _chartArea.SegmentSize;
            int height = _chartArea.Height;

            IDataAdapter dataAdapter = new EventsApiDataAdapter(container)
            {
                ChartHeight = height,
                Offset = start
            };

            var figures = dataAdapter.GetFiguresToDraw(start, start + segmentSize * _chartArea.Width, segmentSize);
            
            Debug.WriteLine("Densities were calculated for {0}ms", stopwatch.ElapsedMilliseconds);

            if (_path == null)
            {
                _path = new Path()
                {
                    Stroke = _lineBrush,
                    StrokeThickness = 1,
                    Fill = _lineBrush
                };
                _chartArea.AddToView(_path);
            }
            

            StreamGeometryDrawingContext context;
            using (context = new StreamGeometryDrawingContext())
            {
                foreach (var figure in figures)
                {
                    figure.Draw(context);
                }
            }

            var geometry = context.Geometry;
            geometry.Freeze();
            _path.Data = geometry;
        }

        /*
        private PathFigure CreatePathFigure(double[] densities, double height)
        {
            if (densities.Length < 1)
                return null;

            var segment = new PolyLineSegment();
            var points = segment.Points;
            double currentValue = 0, prevValue = 0;
            int chartHeight = (int)height;
            for (var i = 0; i < densities.Length; i++)
            {
                if (densities[i] < 0 || densities[i] > 1)
                    throw new ArithmeticException("Density must belong to the range [0; 1]");

                currentValue = chartHeight - (int)(densities[i] * chartHeight);
                if (i > 0 && prevValue != currentValue)
                {
                    points.Add(new Point(i - 1, currentValue));
                }
                points.Add(new Point(i, currentValue));
                
                prevValue = currentValue;
            }

            points.Add(new Point(densities.Length - 1, chartHeight));
            Point startPoint = new Point(0, chartHeight);
            var figure = new PathFigure(startPoint, new[] { segment }, true);

            return figure;
        }*/
    }
}
