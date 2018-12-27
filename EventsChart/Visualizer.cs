﻿using System;
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
        private Brush _lineBrush = new SolidColorBrush(Colors.Coral);

        public Visualizer(IChartArea chartArea)
        {
            _chartArea = chartArea;
        }

        public async Task Redraw()
        {
            var container = _chartArea.BucketContainer;
            if (container == null)
                return;

            long start = container.FirstTimestamp + _chartArea.Offset;
            long segmentSize = _chartArea.SegmentSize;
            int height = _chartArea.Height;

            var densities = await Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();
                double[] result = container.GetDensities(start, start + segmentSize * _chartArea.Width, segmentSize);
                stopwatch.Stop();
                Debug.WriteLine("Densities were calculated for {0}ms", stopwatch.ElapsedMilliseconds);
                return result;
            });

            var stopwatch1 = Stopwatch.StartNew();
            _chartArea.Canvas.Children.Clear();
            for (var i = 0; i < densities.Length; i++)
            {
                if (densities[i] == 0)
                    continue;

                if (densities[i] < 0 || densities[i] > 1)
                    throw new ArithmeticException("Density must belong to the range [0; 1]");

                int lineHeight = (int)(densities[i] * height);
                var line = new Line()
                {
                    Stroke = _lineBrush,
                    X1 = i,
                    X2 = i,
                    Y1 = height,
                    Y2 = height - lineHeight
                };

                _chartArea.AddToView(line);
            }
            stopwatch1.Stop();
            Debug.WriteLine("Canvas was repainted in {0}ms", stopwatch1.ElapsedMilliseconds);
        }

        private PathFigure CreatePathFigure(double[] densities, double height)
        {
            if (densities.Length < 1)
                return null;
            Point startPoint = new Point(0, 0);

            var segment = new PolyLineSegment();
            var points = segment.Points;
            double currentValue = 0, prevValue = 0;
            for (var i = 0; i < densities.Length; i++)
            {
                currentValue = (int)(densities[i] * height);
                if (i > 0 && prevValue != currentValue)
                {
                    points.Add(new Point(i - 1, currentValue));
                }
                points.Add(new Point(i, currentValue));
                
                prevValue = currentValue;
            }


            var figure = new PathFigure(startPoint, new[] { segment }, true);

            return figure;

        }
    }
}