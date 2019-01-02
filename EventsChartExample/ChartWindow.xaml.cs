using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EventsChartExample
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        private DispatcherTimer _timer;
        public BucketContainer BucketContainer { get; }
        public long SegmentSize
        {
            get { return (long)GetValue(SegmentSizeProperty); }
            set { SetValue(SegmentSizeProperty, value); }
        }
        

        public ChartWindow(BucketContainer bucketContainer)
        {
            BucketContainer = bucketContainer;
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 3),
                IsEnabled = true
            };
            _timer.Tick += (obj, args) => HideZoomInfo();
        }

        private void ShowZoomInfo()
        {
            _timer.Stop();
            ZoomInfoLabel.Content = $"Segment size: {SegmentSize}";
            ZoomInfoPanel.Visibility = Visibility.Visible;
            _timer.Start();
        }

        private void HideZoomInfo()
        {
            ZoomInfoPanel.Visibility = Visibility.Collapsed;
            _timer.Stop();
        }




        private static void OnSegmentSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ChartWindow ch)
            {
                ch.ShowZoomInfo();
            }
        }

        public static readonly DependencyProperty SegmentSizeProperty = DependencyProperty.Register(
            nameof(SegmentSize), typeof(long), typeof(ChartWindow), new PropertyMetadata(-1L, OnSegmentSizeChanged));


    }
}
