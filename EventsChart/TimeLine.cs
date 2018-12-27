using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EventsChart
{
    internal class TimeLine : FrameworkElement
    {
        private const int LabelIntervalWidth = 50;
        private readonly Pen _defaultPen = new Pen(Brushes.Blue, 0.5);

        public TimeLine()
        {
            SizeChanged += (o, e) =>
            {
                InvalidateVisual();
            };
        }

        private static PropertyChangedCallback Redraw()
        {
            return (o, args) =>
            {
                if (o is TimeLine timeLine)
                {
                    timeLine.InvalidateVisual();
                }
            };
        }

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(long), typeof(TimeLine), new PropertyMetadata(-1L, Redraw()));

        public long SegmentSize
        {
            get { return (long)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset), typeof(long), typeof(TimeLine), new PropertyMetadata(0L, Redraw()));

        public long Offset
        {
            get { return (long)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }


        


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (SegmentSize <= 0)
                return;

            int width = (int)ActualWidth;
            var segmentSize = SegmentSize;
            bool disableExtendedLabels = false;
            long labelIntervalDuration = segmentSize * LabelIntervalWidth;
            if (TimeUnitHelpers.GetCeilingTimeUnit(labelIntervalDuration).GetTimeUnitDuration() % labelIntervalDuration != 0)
                disableExtendedLabels = true;

            long start = Offset;
            long end = start + segmentSize * width;
            
            int labelCount = (byte)((end - start) / (double)labelIntervalDuration);
            TimeUnit displayedTimeUnit = !disableExtendedLabels 
                ? TimeUnitHelpers.GetFloorTimeUnit(segmentSize) 
                : TimeUnitHelpers.GetCeilingTimeUnit(segmentSize);

            long firstLabelTime = start - start % labelIntervalDuration;
            double firstLabelPosition = -(start % labelIntervalDuration) / (double)labelIntervalDuration * LabelIntervalWidth;
            for (var i = 0; i < labelCount + 2; i++)
            {
                long labelTime = firstLabelTime + i * labelIntervalDuration;
                if (labelTime < 0)
                    continue;
                double posX = firstLabelPosition + i * LabelIntervalWidth;
                var label = new FormattedText(GetDisplayedText(labelTime, displayedTimeUnit), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Courier new"), 11, Brushes.Blue, 1.25);

                drawingContext.DrawText(label, new Point(posX + 3, 0));
                drawingContext.DrawLine(_defaultPen, new Point(posX, 0), new Point(posX, ActualHeight));
            }
            
            
        }


        private string GetDisplayedText(long labelTime, TimeUnit minTimeUnit)
        {
            if (labelTime == 0)
                return "0";


            var bestTimeUnit = TimeUnit.Hour;
            long bestTimeUnitDuration;
            while (labelTime % (bestTimeUnitDuration = bestTimeUnit.GetTimeUnitDuration()) > 0 && bestTimeUnit > minTimeUnit)
                bestTimeUnit = (TimeUnit)((byte)bestTimeUnit >> 1);

            if (labelTime / (double)bestTimeUnitDuration < 1)
                return "";

            return $"{labelTime / bestTimeUnitDuration % bestTimeUnitDuration}{bestTimeUnit.GetTimeUnitAsString()}";
        }
    }
}
