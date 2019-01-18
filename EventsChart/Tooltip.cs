using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EventsChart
{
    class Tooltip : UserControl
    {
        private readonly IChartArea _chartArea;
        private readonly Canvas _canvas;
        private Canvas _tooltipCanvas;
        private Label _currentTimeInfo;
        private Line _marker;
        private const double TooltipHeight = 50;
        private const double TooltipWidth = 150;


        public Tooltip(IChartArea chartArea)
        {
            _chartArea = chartArea;
            _canvas = new Canvas();
            Init();
            Hide();
        }

        private void Init()
        {
            Content = _canvas;

            _tooltipCanvas = new Canvas() {
                Height = TooltipHeight,
                Width = TooltipWidth
            };
            _currentTimeInfo = new Label() {   };
            _tooltipCanvas.Children.Add(new Rectangle() {
                Height = TooltipHeight,
                Width = TooltipWidth,
                Opacity = 0.75,
                Fill = Brushes.Snow
            });
            _tooltipCanvas.Children.Add(_currentTimeInfo);
            
            Canvas.SetTop(this, 0);
            _canvas.Children.Add(_tooltipCanvas);

            _marker = new Line() {
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                X1 = 0,
                Y1 = 0,
                X2 = 0
            };
            _canvas.Children.Add(_marker);


        }

        public void Show(double x, double y, int eventsCount)
        {
            Visibility = System.Windows.Visibility.Visible;

            double posX = 0, posY = y;
            if (y + TooltipHeight > _chartArea.Height)
                posY -= TooltipHeight;

            if (x + TooltipWidth > _chartArea.Width)
                posX = -TooltipWidth;


            Canvas.SetLeft(this, x);

            Canvas.SetLeft(_tooltipCanvas, posX);
            Canvas.SetTop(_tooltipCanvas, posY);

            _canvas.Height = _chartArea.Height;
            _marker.Y2 = _chartArea.Height;

            var currentTime = TimeSpan.FromTicks((long)(_chartArea.SegmentSize.DisplayedValue * x) + _chartArea.Offset);
            _currentTimeInfo.Content = $"Time: {currentTime}\nEvent count: {eventsCount}";
            
        }

        public void Hide()
        {
            Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
