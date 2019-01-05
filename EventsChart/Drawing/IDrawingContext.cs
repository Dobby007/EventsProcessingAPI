using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EventsChart.Drawing
{
    /// <summary>
    /// Bridge pattern (Implementation)
    /// </summary>
    interface IDrawingContext
    {
        void DrawRectangle(Rect rect);
        void DrawPolyline(Point start, IList<Point> points);
        void DrawLine(Point start, Point end);
    }
}
