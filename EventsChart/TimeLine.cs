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

        private double LabelIntervalWidth;
        private long LabelIntervalDuration;
        private TimeUnit LabelIntervalUnit;

        public TimeLine()
        {
            _defaultPen.Freeze();
            SizeChanged += (o, e) =>
            {
                InvalidateVisual();
            };
        }

        

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(SegmentSize), typeof(TimeLine), new PropertyMetadata(new SegmentSize(-1L), OnSegmentSizeChanged));

        public SegmentSize SegmentSize
        {
            get { return (SegmentSize)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset), typeof(long), typeof(TimeLine), new PropertyMetadata(0L, OnOffsetChanged));

        public long Offset
        {
            get { return (long)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        private static void OnSegmentSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TimeLine timeLine)
            {
                timeLine.SetLabelIntervalParameters();
                timeLine.InvalidateVisual();
            }
        }

        private static void OnOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is TimeLine timeLine)
            {
                timeLine.InvalidateVisual();
            }
        }

        
        private void SetLabelIntervalParameters()
        {
            long segmentSize = SegmentSize.DisplayedValue;
            long minLabelIntervalDuration = segmentSize * MinLabelIntervalWidthInPixels;
            (double Value, TimeUnit Unit) minIntervalDurationInUnits = TimeUnitHelpers.ConvertTicksToTime(minLabelIntervalDuration);
            if (!TimeUnitFactors.TryGetBestTimeFactor(minIntervalDurationInUnits.Value, minIntervalDurationInUnits.Unit, out double factor))
                return;

            LabelIntervalWidth = factor * MinLabelIntervalWidthInPixels / minIntervalDurationInUnits.Value;
            LabelIntervalDuration = (long)factor * minIntervalDurationInUnits.Unit.GetTimeUnitDuration();
            LabelIntervalUnit = TimeUnitHelpers.GetFloorTimeUnit(LabelIntervalDuration);
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            long segmentSize = SegmentSize.DisplayedValue;
            double labelIntervalWidth = LabelIntervalWidth;
            long labelIntervalDuration = LabelIntervalDuration;
            TimeUnit labelIntervalUnit = LabelIntervalUnit;

            if (segmentSize <= 0)
                return;
            
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
                    
                drawingContext.DrawText(label, new Point(posX + 3, 0));*/
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
                if (currentTimeUnit == TimeUnit.Hour)
                    break;
                currentTimeUnit = (TimeUnit)((byte)currentTimeUnit << 1);
            }
            while (labelTime % (currentDuration = currentTimeUnit.GetTimeUnitDuration()) == 0);

            if (labelTime % currentDuration == 0)
                return $"{labelTime / bestTimeUnitDuration}{bestTimeUnit.GetTimeUnitAsString()}";

            return $"{labelTime % currentDuration / bestTimeUnitDuration}{bestTimeUnit.GetTimeUnitAsString()}";
        }


        #region Text rendering
        private static Dictionary<int, (ushort Index, double Width)> _glyphCache = new Dictionary<int, (ushort, double)>();
        private static GlyphTypeface _glyphTypeface;

        public static GlyphRun CreateGlyphRun(string text, double size, Point position)
        {
            if (_glyphTypeface == null)
            {
                Typeface typeface = new Typeface("Arial");
                if (!typeface.TryGetGlyphTypeface(out _glyphTypeface))
                    throw new InvalidOperationException("No glyphtypeface found");
            }

            ushort[] glyphIndexes = new ushort[text.Length];
            double[] advanceWidths = new double[text.Length];
            
            double glyphWidth;

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex;
                if (!_glyphCache.TryGetValue(text[n], out var glyphInfo))
                {
                    if (!_glyphTypeface.CharacterToGlyphMap.TryGetValue(text[n], out glyphIndex))
                        continue;
                    glyphWidth = _glyphTypeface.AdvanceWidths[glyphIndex] * size;
                    _glyphCache.Add(text[n], (glyphIndex, glyphWidth));
                }
                else
                {
                    glyphIndex = glyphInfo.Index;
                    glyphWidth = glyphInfo.Width;
                }
                
                glyphIndexes[n] = glyphIndex;
                advanceWidths[n] = glyphWidth;
            }

            var offsetPosition = new Point(position.X, position.Y - 10 - size);
            GlyphRun glyphRun = new GlyphRun(_glyphTypeface, 0, false, size, 1.25F, glyphIndexes, offsetPosition, advanceWidths, null, null, null, null, null, null);
            return glyphRun;
        }
        #endregion
    }
}
