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
using EventsProcessingAPI.Enumeration;

namespace EventsChart.ChartData
{
    class EventsApiDataAdapter : IDataAdapter
    {
        private readonly BucketContainer _container;
        public double ChartHeight { get; set; }
        public long Offset { get; set; }

        public EventsApiDataAdapter(BucketContainer container)
        {
            _container = container;
        }

        public IEnumerable<IFigure> GetFiguresToDraw(long start, long end, long segmentSize)
        {
            if (segmentSize > 1)
            {
                double[] densities = _container.GetDensities(start, end, segmentSize);
                return GetFiguresFromDensities(densities);
            }
            else if (segmentSize == 1)
            {
                var events = _container.GetRealEvents(start, end);
                return GetFiguresFromEvents(events, end);
            }

            return Enumerable.Empty<IFigure>();
        }

        private IEnumerable<IFigure> GetFiguresFromEvents(RealEventEnumerable events, long endTimestamp)
        {
            long offset = Offset;
            double height = ChartHeight;
            long startTime = -1, duration = 0;
            int eventsCount = 0;

            var figures = new List<IFigure>();

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case EventType.Start:
                        startTime = ev.Ticks;
                        break;
                    case EventType.Stop:
                        duration = ev.Ticks - startTime;
                        figures.Add(new Rectangle(
                            new Rect(Math.Max(startTime - offset, 0), 0, duration, height)
                        ));
                        startTime = -1;
                        break;
                }
                eventsCount++;
            }

            if (startTime >= 0)
            {
                duration = endTimestamp - startTime;
                figures.Add(new Rectangle(new Rect(Math.Max(startTime - offset, 0), 0, duration, height)));
            }

            return figures;
        }

        private IEnumerable<IFigure> GetFiguresFromDensities(double[] densities)
        {
            int chartHeight = (int)ChartHeight;
            double currentValue = 0, prevValue = 0;
            Point startPoint = default;
            bool isNewFigure = true;
            FigureType figureType = FigureType.Unknown;

            var points = new List<Point>(densities.Length + 5 /* с запасом */);
            for (var i = 0; i < densities.Length; i++)
            {
                if (densities[i] < 0 || densities[i] > 1)
                    throw new ArithmeticException("Density must belong to the range [0; 1]");

                currentValue = chartHeight - (int)(densities[i] * chartHeight);

                // if density ~ 0
                if (currentValue == chartHeight)
                {
                    SetFigureType(ref figureType, points);
                    if (points.Count > 0)
                    {
                        points.Add(new Point(i - 1, currentValue));
                        yield return CreateFigure(figureType, startPoint, points);
                        points.Clear();
                        isNewFigure = true;
                        figureType = FigureType.Unknown;
                    }
                    
                    continue;
                }

                if (isNewFigure)
                {
                    startPoint = new Point(i, chartHeight);
                }

                if (!isNewFigure)
                {
                    if (prevValue < currentValue)
                    {
                        points.Add(new Point(i - 1, currentValue));
                        figureType = FigureType.Polyline;
                    }
                    else if (prevValue > currentValue)
                    {
                        points.Add(new Point(i, prevValue));
                        figureType = FigureType.Polyline;
                    }
                }
                points.Add(new Point(i, currentValue));

                isNewFigure = false;

                prevValue = currentValue;
            }

            SetFigureType(ref figureType, points);

            if (points.Count > 0)
            {
                points.Add(new Point(densities.Length - 1, chartHeight));
                yield return CreateFigure(figureType, startPoint, points);
            }
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
