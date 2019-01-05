using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EventsChart.Drawing
{
    class StreamGeometryDrawingContext : IDrawingContext, IDisposable
    {
        private readonly StreamGeometry _geometry;
        private readonly StreamGeometryContext _geometryContext;

        public StreamGeometry Geometry => _geometry;

        public StreamGeometryDrawingContext()
        {
            _geometry = new StreamGeometry();
            _geometryContext = _geometry.Open();
        }

        public void DrawLine(Point start, Point end)
        {
            _geometryContext.BeginFigure(start, false, false);
            _geometryContext.LineTo(end, true, false);
        }

        public void DrawPolyline(Point start, IList<Point> points)
        {
            _geometryContext.BeginFigure(start, true, true);
            _geometryContext.PolyLineTo(points, false, true);
        }

        public void DrawRectangle(Rect rect)
        {
            _geometryContext.BeginFigure(rect.BottomLeft, true, true);
            _geometryContext.LineTo(rect.TopLeft, false, false);
            _geometryContext.LineTo(rect.TopRight, false, true);
            _geometryContext.LineTo(rect.BottomRight, false, true);

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ((IDisposable)_geometryContext).Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
