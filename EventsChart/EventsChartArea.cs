using EventsChart.ChartData;
using EventsProcessingAPI;
using EventsProcessingAPI.Common;
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
    internal class EventsChartArea : UserControl, IChartArea, IFigureDataAdapterFactory
    {
        public Canvas Canvas { get; }
        private readonly ChartUpdater _chartUpdater;
        private IFigureDataAdapter _figureDataAdapter;

        public EventsChartArea()
        {
            Canvas = new Canvas() {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Content = Canvas;

            _chartUpdater = new ChartUpdater(this, this);

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

        public SegmentSize SegmentSize
        {
            get { return (SegmentSize)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }

        int IChartArea.Width => (int)ActualWidth;

        int IChartArea.Height => (int)ActualHeight;
        #endregion

        #region Events
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitChart();
            
            await UpdateChart();
        }

        private async void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (!IsLoaded)
                return;
            
            InitChart();
            await UpdateChart();
        }

        private static async void OnBucketContainerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var wpfChart = obj as EventsChartArea;
            if (wpfChart == null) return;
            await wpfChart.UpdateChart();
        }
        #endregion

        private async Task UpdateChart()
        {
            if (!IsVisible || !IsLoaded || SegmentSize.DisplayedValue <= 0)
                return;

            await _chartUpdater.Run();
        }

        private void InitChart()
        {
            _figureDataAdapter = new FigureDataAdapterForApi(BucketContainer, ActualHeight);
            //Canvas.Clip = new RectangleGeometry(new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight)));
        }

        public void AddToView(FrameworkElement element)
        {
            Canvas.Children.Add(element);
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

        IFigureDataAdapter IFigureDataAdapterFactory.Get()
        {
            return _figureDataAdapter;
        }
    }
}
