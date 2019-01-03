using EventsProcessingAPI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace EventsChart
{
    /// <summary>
    /// Interaction logic for EventsChart.xaml
    /// </summary>
    public partial class EventsChart : UserControl, IScrollInfo
    {
        private long[] _prefferedSegmentSizes = Array.Empty<long>();

        public EventsChart()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public bool ZoomIn()
        {
            var currentSegmentSize = SegmentSize;
            var next = Array.FindIndex(_prefferedSegmentSizes, s => s == currentSegmentSize) - 1;
            if (next >= 0)
            {
                SegmentSize = _prefferedSegmentSizes[next];
                return true;
            }
            return false;
        }

        public bool ZoomOut()
        {
            var currentSegmentSize = SegmentSize;
            var next = Array.FindIndex(_prefferedSegmentSizes, s => s == currentSegmentSize) + 1;
            if (next < _prefferedSegmentSizes.Length)
            {
                SegmentSize = _prefferedSegmentSizes[next];
                return true;
            }
            return false;
        }

        public void ZoomOutMax()
        {
            if (_prefferedSegmentSizes.Length > 0)
                SegmentSize = _prefferedSegmentSizes[_prefferedSegmentSizes.Length - 1];
        }

        public void ZoomInMax()
        {
            if (_prefferedSegmentSizes.Length > 0)
                SegmentSize = _prefferedSegmentSizes[0];
        }

        private void SetPrefferedSegmentSizes()
        {
            var bucketContainer = BucketContainer;
            if (ActualWidth == 0 || bucketContainer == null)
                return;

            _prefferedSegmentSizes = bucketContainer.GetPreferredSegmentSizes((ushort)ActualWidth);
        }

        #region Properties
        public static readonly DependencyProperty BucketContainerProperty = DependencyProperty.Register(
            nameof(BucketContainer), typeof(BucketContainer), typeof(EventsChart), new PropertyMetadata(OnBucketContainerChanged));

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset), typeof(long), typeof(EventsChart), new PropertyMetadata(-1L, null, CoerceOffset));

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(long), typeof(EventsChart), new PropertyMetadata(-1L, OnSegmentSizeChanged, CoerceSegmentSize));


        public BucketContainer BucketContainer
        {
            get { return (BucketContainer)GetValue(BucketContainerProperty); }
            set { SetValue(BucketContainerProperty, value); }
        }

        public long Offset
        {
            get { return (long)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public long SegmentSize
        {
            get { return (long)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }

        
        #endregion

        #region Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetPrefferedSegmentSizes();
            // We display the whole chart by default
            if (SegmentSize == -1)
                ZoomOutMax();

            UpdateScrollInfo();
        }

        private static void OnBucketContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is EventsChart chart)
            {
                var val = args.NewValue as BucketContainer;
                chart.SetPrefferedSegmentSizes();
                chart.UpdateScrollInfo();
            }

            d.CoerceValue(OffsetProperty);
            d.CoerceValue(SegmentSizeProperty);
        }

        private static void OnSegmentSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is EventsChart chart)
            {
                chart.UpdateScrollInfo();
            }

            d.CoerceValue(OffsetProperty);
        }

        

        private static object CoerceOffset(DependencyObject d, object value)
        {
            long offset = (long)value;
            if (d is EventsChart chart)
            {
                var container = chart.BucketContainer;
                if (container == null || offset == -1)
                    return offset;

                long lastTimestamp = container.LastTimestamp;
                long viewportDuration = (long)(chart.ActualWidth * chart.SegmentSize);
                long lastPossibleTimestamp = (long)Math.Ceiling((container.LastTimestamp - container.FirstTimestamp) / (double)chart.SegmentSize) 
                    * chart.SegmentSize;

                return Math.Max(Math.Min(offset, lastPossibleTimestamp - viewportDuration), 0);
            }
            return offset;
        }

        private static object CoerceSegmentSize(DependencyObject d, object value)
        {
            long current = (long)value;

            if (d is EventsChart chart)
            {
                if (current == -1 || chart.BucketContainer == null)
                    return current;

                if (chart._prefferedSegmentSizes.Length > 0 && !chart._prefferedSegmentSizes.Contains(current))
                    return 1L;
            }

            return current;
        }
        #endregion

        #region IScrollInfo support
        
        private double Step => 5;
        private double WheelSize => 3 * Step;
        
        private Size _extent;
        private Size _viewport;
        private Point _scrollOffset;

        public bool CanVerticallyScroll { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public double ExtentHeight => _extent.Height;

        public double ExtentWidth => _extent.Width;

        public double HorizontalOffset => _scrollOffset.X;

        public double VerticalOffset => _scrollOffset.Y;

        public double ViewportHeight => _viewport.Height;

        public double ViewportWidth => _viewport.Width;

        public ScrollViewer ScrollOwner { get; set; }

        #region Keyboard and mouse
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + Step);
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - Step);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - Step);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + Step);
        }

        public void MouseWheelDown()
        {
            ZoomOut();
        }

        public void MouseWheelUp()
        {
            ZoomIn();
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - WheelSize);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + WheelSize);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }
        #endregion

        public void SetHorizontalOffset(double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
            if (offset != _scrollOffset.X)
            {
                long segmentSize = SegmentSize;
                Offset = (long)offset * segmentSize;
                _scrollOffset = new Point(Offset / segmentSize, _scrollOffset.Y);
            }
        }

        public void SetVerticalOffset(double offset)
        {
            
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return Rect.Empty;
        }

        private void UpdateScrollInfo()
        {
            if (BucketContainer == null)
                return;

            long start = BucketContainer.FirstTimestamp;
            long end = BucketContainer.LastTimestamp;
            if (end < start)
                return;

            double segmentSize = Math.Max(SegmentSize, 1);
            Size viewport = new Size(ActualWidth, 1);
            Size extent = new Size(Math.Ceiling((end - start) / segmentSize), 1);
            Point scrollOffset = new Point(Math.Max((Offset - start) / segmentSize, 0), 0);

            _extent = extent;
            _viewport = viewport;
            _scrollOffset = scrollOffset;

            ScrollOwner?.InvalidateScrollInfo();
        }
        #endregion

    }
}
