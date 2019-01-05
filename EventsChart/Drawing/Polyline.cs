using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EventsChart.Drawing
{
    class Polyline : IFigure
    {
        private readonly IList<Point> _points;
        private readonly Point _startPoint;

        public Polyline(Point startPoint, IList<Point> points)
        {
            _points = points;
            _startPoint = startPoint;
        }

        public void Draw(IDrawingContext context)
        {
            context.DrawPolyline(_startPoint, _points);
        }
    }
}
