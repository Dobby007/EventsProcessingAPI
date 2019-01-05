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
        private const int MinLabelIntervalWidthInPixels = 50;
        private readonly Pen _defaultPen = new Pen(Brushes.Blue, 0.5);

        public TimeLine()
        {
            _defaultPen.Freeze();
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

            long segmentSize = SegmentSize;
            long minLabelIntervalDuration = segmentSize * MinLabelIntervalWidthInPixels;
            (double Value, TimeUnit Unit) minIntervalDurationInUnits = TimeUnitHelpers.ConvertTicksToTime(minLabelIntervalDuration);
            if (!TimeUnitFactors.TryGetBestTimeFactor(minIntervalDurationInUnits.Value, minIntervalDurationInUnits.Unit, out double factor))
                return;
            
            double labelIntervalWidth = factor * MinLabelIntervalWidthInPixels / minIntervalDurationInUnits.Value;
            long labelIntervalDuration = (long)factor * minIntervalDurationInUnits.Unit.GetTimeUnitDuration();
            TimeUnit labelIntervalUnit = TimeUnitHelpers.GetFloorTimeUnit(labelIntervalDuration);

            int areaWidth = (int)ActualWidth;

            long start = Offset;
            long end = start + segmentSize * areaWidth;
            
            int labelCount = (byte)((end - start) / (double)labelIntervalDuration);

            long firstLabelTime = start - start % labelIntervalDuration;
            double firstLabelPosition = -(start % labelIntervalDuration) / (double)labelIntervalDuration * labelIntervalWidth;
            for (var i = 0; i < labelCount + 2; i++)
            {
                long labelTime = firstLabelTime + i * labelIntervalDuration;
                if (labelTime < 0)
                    continue;
                double posX = firstLabelPosition + i * labelIntervalWidth;
                /*var label = new FormattedText(GetDisplayedText(labelTime, labelIntervalUnit), CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, new Typeface("Courier new"), 11, Brushes.Blue, 1.25);
                    */
                //drawingContext.DrawText(label, new Point(posX + 3, 0));
                drawingContext.DrawGlyphRun(Brushes.Blue, CreateGlyphRun(GetDisplayedText(labelTime, labelIntervalUnit), 11, new Point(posX + 3, ActualHeight)));
                drawingContext.DrawLine(_defaultPen, new Point(posX, 0), new Point(posX, ActualHeight));
            }
            
            
        }


        private string GetDisplayedText(long labelTime, TimeUnit minTimeUnit)
        {
            if (labelTime == 0)
                return "0";

            
            TimeUnit bestTimeUnit, currentTimeUnit = minTimeUnit;
            long bestTimeUnitDuration, currentDuration = minTimeUnit.GetTimeUnitDuration();

            do
            {
                bestTimeUnitDuration = currentDuration;
                bestTimeUnit = currentTimeUnit;
                currentTimeUnit = (TimeUnit)((byte)currentTimeUnit << 1);
            }
            while (labelTime % (currentDuration = currentTimeUnit.GetTimeUnitDuration()) == 0);

            

            return $"{labelTime % currentDuration / bestTimeUnitDuration}{bestTimeUnit.GetTimeUnitAsString()}";
        }


        private static Dictionary<ushort, double> _glyphWidths = new Dictionary<ushort, double>();
        private static GlyphTypeface _glyphTypeface;
        public static GlyphRun CreateGlyphRun(string text, double size, Point position)
        {
            if (_glyphTypeface == null)
            {
                Typeface typeface = new Typeface("Courier new");
                if (!typeface.TryGetGlyphTypeface(out _glyphTypeface))
                    throw new InvalidOperationException("No glyphtypeface found");
            }

            ushort[] glyphIndexes = new ushort[text.Length];
            double[] advanceWidths = new double[text.Length];

            var totalWidth = 0d;
            double glyphWidth;

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex = (ushort)(text[n] - 29);
                glyphIndexes[n] = glyphIndex;

                if (!_glyphWidths.TryGetValue(glyphIndex, out glyphWidth))
                {
                    glyphWidth = _glyphTypeface.AdvanceWidths[glyphIndex] * size;
                    _glyphWidths.Add(glyphIndex, glyphWidth);
                }
                advanceWidths[n] = glyphWidth;
                totalWidth += glyphWidth;
            }

            var offsetPosition = new Point(position.X - (totalWidth / 2), position.Y - 10 - size);

            GlyphRun glyphRun = new GlyphRun(_glyphTypeface, 0, false, size, 1.25F, glyphIndexes, offsetPosition, advanceWidths, null, null, null, null, null, null);

            return glyphRun;
        }
    }
}
