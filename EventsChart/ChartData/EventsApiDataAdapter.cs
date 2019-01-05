using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EventsChart.Drawing;
using EventsDomain;
using EventsProcessingAPI;

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
                return GetFiguresFromEvents(events);
            }

            return Enumerable.Empty<IFigure>();
        }

        private IEnumerable<IFigure> GetFiguresFromEvents(RealEventEnumerable events)
        {
            long offset = Offset;
            double height = ChartHeight;
            long startTime = -1;
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
                        long duration = ev.Ticks - startTime;
                        var x = startTime - offset;
                        figures.Add(new Rectangular(new Rect(x, height, duration, 0)));
                        startTime = -1;
                        break;
                }
                eventsCount++;
            }

            return figures;
        }

        private IEnumerable<IFigure> GetFiguresFromDensities(double[] densities)
        {
            int chartHeight = (int)ChartHeight;
            double currentValue = 0, prevValue = 0;
            Point startPoint = default;
            bool isNewFigure = true;

            var points = new List<Point>(densities.Length + 5 /* с запасом */);
            for (var i = 0; i < densities.Length; i++)
            {
                if (densities[i] < 0 || densities[i] > 1)
                    throw new ArithmeticException("Density must belong to the range [0; 1]");

                currentValue = chartHeight - (int)(densities[i] * chartHeight);

                // if density ~ 0
                if (currentValue == chartHeight)
                {
                    if (points.Count > 0)
                    {
                        points.Add(new Point(i - 1, currentValue));
                        yield return new Polyline(startPoint, points);
                        points.Clear();
                        isNewFigure = true;
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
                    }
                    else if (prevValue > currentValue)
                    {
                        points.Add(new Point(i, prevValue));
                    }
                }
                points.Add(new Point(i, currentValue));

                isNewFigure = false;

                prevValue = currentValue;
            }

            if (points.Count > 0)
            {
                points.Add(new Point(densities.Length - 1, chartHeight));
                yield return new Polyline(startPoint, points);
            }
            
        }
    }
}
