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

namespace EventsChart.Rendering
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

        public void Redraw(IEnumerable<IFigure> figures)
        {
            if (_path == null)
            {
                _path = new Path()
                {
                    Stroke = _lineBrush,
                    StrokeThickness = 1,
                    Fill = _lineBrush
                };
                _chartArea.AddToArea(_path);
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
        
    }
}
