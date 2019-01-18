using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EventsChart.Drawing;
using EventsDomain;
using EventsProcessingAPI;
using EventsProcessingAPI.Common;
using EventsProcessingAPI.Enumeration;

namespace EventsChart.ChartData
{
    class FigureDataAdapterForApi : IFigureDataAdapter
    {
        private readonly BucketContainer _container;
        public readonly double _chartHeight;
        public readonly double[] _targetBuffer;
        private readonly List<Point> _points = new List<Point>();

        public FigureDataAdapterForApi(BucketContainer container, double chartHeight, double[] targetBuffer)
        {
            _container = container;
            _chartHeight = chartHeight;
            _targetBuffer = targetBuffer;
        }

        public IEnumerable<IFigure> GetFiguresToDraw(long offset, long width, SegmentSize segmentSize)
        {
            long firstTimestamp = _container.FirstTimestamp;
            long start = firstTimestamp + offset;
            long end = firstTimestamp + offset + width * segmentSize.DisplayedValue;

            if (segmentSize.RequestedValue > 1)
            {
                Span<double> densities = _container.GetDensities(start, end, segmentSize.RequestedValue, _targetBuffer);
                return GetFiguresFromDensities(_targetBuffer, 0, densities.Length);
            }
            else if (segmentSize.RequestedValue == 1)
            {
                var events = _container.GetRealEvents(start, end + 1);
                return GetFiguresFromEvents(events, offset, start, end);
            }

            return Enumerable.Empty<IFigure>();
        }

        private IEnumerable<IFigure> GetFiguresFromEvents(RealEventEnumerable events, long offset, long startTimestamp, long endTimestamp)
        {
            long startEventTime = -1, duration = 0;
            int eventsCount = 0;

            var figures = new List<IFigure>();

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case EventType.Start:
                        startEventTime = ev.Ticks;
                        break;
                    case EventType.Stop:
                        if (startEventTime < 0)
                            startEventTime = startTimestamp;

                        duration = ev.Ticks - startEventTime;
                        figures.Add(CreateFigure(Math.Max(startEventTime - startTimestamp, 0), 0, duration, _chartHeight));
                        startEventTime = -1;
                        break;
                }
                eventsCount++;
            }

            if (startEventTime >= 0)
            {
                duration = endTimestamp - startEventTime;
                figures.Add(CreateFigure(Math.Max(startEventTime - startTimestamp, 0), 0, duration, _chartHeight));
            }

            return figures;
        }

        private IEnumerable<IFigure> GetFiguresFromDensities(double[] buffer, int indexFrom, int length)
        {
            double currentValue = 0, prevValue = 0;
            Point startPoint = default;
            bool isNewFigure = true;
            FigureType figureType = FigureType.Unknown;
            
            
            for (var i = indexFrom; i < length; i++)
            {
                if (buffer[i] < 0 || buffer[i] > 1)
                    throw new ArithmeticException("Density must belong to the range [0; 1]");

                currentValue = _chartHeight - (int)(buffer[i] * _chartHeight);

                // if density ~ 0
                if (currentValue == _chartHeight)
                {
                    SetFigureType(ref figureType, _points);
                    if (_points.Count > 0)
                    {
                        _points.Add(new Point(i - 1, currentValue));
                        yield return CreateFigure(figureType, startPoint, _points);
                        _points.Clear();
                        isNewFigure = true;
                        figureType = FigureType.Unknown;
                    }
                    
                    continue;
                }

                if (isNewFigure)
                {
                    startPoint = new Point(i, _chartHeight);
                }

                if (!isNewFigure)
                {
                    if (prevValue < currentValue)
                    {
                        _points.Add(new Point(i - 1, currentValue));
                        figureType = FigureType.Polyline;
                    }
                    else if (prevValue > currentValue)
                    {
                        _points.Add(new Point(i, prevValue));
                        figureType = FigureType.Polyline;
                    }
                }
                _points.Add(new Point(i, currentValue));

                isNewFigure = false;

                prevValue = currentValue;
            }

            SetFigureType(ref figureType, _points);

            if (_points.Count > 0)
            {
                _points.Add(new Point(buffer.Length - 1, _chartHeight));
                yield return CreateFigure(figureType, startPoint, _points);
            }
            _points.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IFigure CreateFigure(FigureType figureType, Point startPoint, IList<Point> points)
        {
            switch (figureType)
            {
                case FigureType.Line:
                    return new Line(startPoint, points[0]);
                case FigureType.Rectangle:
                    return new Rectangle(new Rect(startPoint, points[points.Count - 2]));
                case FigureType.Polyline:
                    return new Polyline(startPoint, points);
                default:
                    throw new ArgumentOutOfRangeException(nameof(figureType), figureType, "Unknown figure type");
            }
        }

        private IFigure CreateFigure(double x, double y, double width, double height)
        {
            if (width == 1)
            {
                return new Line(new Point(x, y), new Point(x, height));
            }

            return new Rectangle(new Rect(x, y, width, height));
        }

        private void SetFigureType(ref FigureType figureType, IList<Point> points)
        {
            if (points.Count == 1)
                figureType = FigureType.Line;
            else if (figureType == FigureType.Unknown)
                figureType = FigureType.Rectangle;
        }


        private enum FigureType
        {
            Unknown,
            Polyline,
            Line,
            Rectangle
        }
    }
}
