using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventsChart
{
    internal class EventsChartArea : UserControl, IChartArea
    {
        public Canvas Canvas { get; }
        private readonly ChartUpdater _chartUpdater;

        public EventsChartArea()
        {
            Canvas = new Canvas() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Content = Canvas;

            _chartUpdater = new ChartUpdater(TimeSpan.FromMilliseconds(1), this);

            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitChart();
            
            await UpdateChart();
        }

        #region Properties
        public static readonly DependencyProperty BucketContainerProperty = DependencyProperty.Register(
            nameof(BucketContainer), typeof(BucketContainer), typeof(EventsChartArea), new PropertyMetadata(OnBucketContainerChanged));

        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            nameof(Offset), typeof(long), typeof(EventsChartArea), new PropertyMetadata(0L, GetChartUpdater()));

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(long), typeof(EventsChartArea), new PropertyMetadata(-1L, GetChartUpdater()));


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

        int IChartArea.Width => (int)ActualWidth;

        int IChartArea.Height => (int)ActualHeight;
        #endregion

        private async void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (!IsLoaded)
                return;

            InitChart();
            await UpdateChart();
        }



        private async Task UpdateChart()
        {
            if (!IsLoaded || SegmentSize <= 0)
                return;
            
            await _chartUpdater.Run();
        }

        private void InitChart()
        {
            Canvas.Clip = new RectangleGeometry(new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
        }
        

        public void AddToView(FrameworkElement element)
        {
            Canvas.Children.Add(element);
        }

        private static async void OnBucketContainerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var wpfChart = obj as EventsChartArea;
            if (wpfChart == null) return;
            await wpfChart.UpdateChart();
        }

        private static PropertyChangedCallback GetChartUpdater()
        {
            return async (o, args) =>
            {
                var wpfChart = o as EventsChartArea;
                if (wpfChart == null) return;
                await wpfChart.UpdateChart();
            };
        }
    }
}
