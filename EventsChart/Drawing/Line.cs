using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EventsChart.Drawing
{
    sealed class Line : IFigure
    {
        private readonly Point _startPoint;
        private readonly Point _endPoint;

        public Line(Point startPoint, Point endPoint)
        {
            _startPoint = startPoint;
            _endPoint = endPoint;
        }
        public void Draw(IDrawingContext context)
        {
            context.DrawLine(_startPoint, _endPoint);
        }
    }
}
