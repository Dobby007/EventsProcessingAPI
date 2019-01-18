using EventsChart.ChartData;
using EventsChart.Rendering;
using EventsProcessingAPI;
using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventsChart
{
    internal class EventsChartArea : UserControl, IChartArea, IFigureDataAdapterFactory
    {
        public Canvas Canvas { get; }
        public Canvas WrappingCanvas { get; }
        private readonly ChartUpdater _chartUpdater;
        private IFigureDataAdapter _figureDataAdapter;
        private double[] _targetBufferForDensities;
        private Tooltip _tooltip;
        private long _firstTimestamp;

        public EventsChartArea()
        {
            WrappingCanvas = new Canvas() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Canvas = new Canvas();
            WrappingCanvas.Children.Add(Canvas);

            Content = WrappingCanvas;

            _chartUpdater = new ChartUpdater(this, this);

            _tooltip = new Tooltip(this);
            AddUiElement(_tooltip);

            MouseMove += EventsChartArea_MouseMove;
            MouseLeave += EventsChartArea_MouseLeave;
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            
        }

        

        #region Properties
        public static readonly DependencyProperty BucketContainerProperty = DependencyProperty.Register(
            nameof(BucketContainer), typeof(BucketContainer), typeof(EventsChartArea), new PropertyMetadata(OnBucketContainerChanged));

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset), typeof(long), typeof(EventsChartArea), new PropertyMetadata(0L, GetChartUpdater()));

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(SegmentSize), typeof(EventsChartArea), new PropertyMetadata(new SegmentSize(-1L), GetChartUpdater()));

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }

        public BucketContainer BucketContainer
        {
            get { return (BucketContainer)GetValue(BucketContainerProperty); }
            set
            {
                SetValue(BucketContainerProperty, value);
            }
        }

        public long Offset
        {
            get { return (long)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public SegmentSize SegmentSize
        {
            get { return (SegmentSize)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }

        int IChartArea.Width => (int)ActualWidth;

        int IChartArea.Height => (int)ActualHeight;
        #endregion

        #region Events
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateAreaSize();
            
            UpdateChart();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (!IsLoaded)
                return;
            
            UpdateAreaSize();
            UpdateChart();
        }

        private static void OnBucketContainerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var wpfChart = obj as EventsChartArea;
            if (wpfChart == null) return;
            wpfChart._firstTimestamp = ((BucketContainer)args.NewValue).FirstTimestamp;
            wpfChart._targetBufferForDensities = ((BucketContainer)args.NewValue).CreateBufferForDensities();
            wpfChart.UpdateChart();
        }

        private void EventsChartArea_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _tooltip.Hide();
        }

        private void EventsChartArea_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var position = e.GetPosition(this);

            long start = _firstTimestamp + Offset + (long)(SegmentSize.DisplayedValue * position.X);
            long end = start + SegmentSize.DisplayedValue + 1;
            var payloads = BucketContainer.GetPayloads(start, end);

            _tooltip.Show(position.X, position.Y, payloads.Count);
        }
        #endregion

        private void UpdateChart()
        {
            if (!IsVisible || !IsLoaded || SegmentSize.DisplayedValue <= 0)
                return;

            _chartUpdater.Run();
        }

        private void UpdateAreaSize()
        {
            _figureDataAdapter = new FigureDataAdapterForApi(BucketContainer, ActualHeight, _targetBufferForDensities);
            Clip = new RectangleGeometry(new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));

            Canvas.SetLeft(Canvas, 0);
            Canvas.SetTop(Canvas, 0);
            Canvas.Width = ActualWidth;
            Canvas.Height = ActualHeight;
        }

        public void AddUiElement(FrameworkElement element)
        {
            WrappingCanvas.Children.Add(element);
        }

        public void AddToArea(FrameworkElement element)
        {
            Canvas.Children.Add(element);
        }

        private static PropertyChangedCallback GetChartUpdater()
        {
            return (o, args) =>
            {
                var wpfChart = o as EventsChartArea;
                if (wpfChart == null) return;
                wpfChart.UpdateChart();
            };
        }

        IFigureDataAdapter IFigureDataAdapterFactory.Get()
        {
            return _figureDataAdapter;
        }
    }
}
