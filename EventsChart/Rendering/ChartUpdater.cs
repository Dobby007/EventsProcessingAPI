using EventsChart.ChartData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace EventsChart.Rendering
{
    internal class ChartUpdater
    {
        private RenderingRequest _lastRenderingRequest;
        private readonly Visualizer _visualizer;
        private readonly IChartArea _chartArea;
        private readonly IFigureDataAdapterFactory _dataAdapterFactory;

        public ChartUpdater(IChartArea chartArea, IFigureDataAdapterFactory dataAdapterFactory)
        {
            _chartArea = chartArea;
            _visualizer = new Visualizer(chartArea);
            _dataAdapterFactory = dataAdapterFactory;
        }


        public void Run()
        {          
            var segmentSize = _chartArea.SegmentSize;
            var offset = _chartArea.Offset;
            var width = _chartArea.Width;

            var dataAdapter = _dataAdapterFactory.Get();
            if (dataAdapter == null)
                return;

            double translateX = 0;
            if (_lastRenderingRequest.Covers(offset, segmentSize, width))
            {
                translateX = (offset - _lastRenderingRequest.Offset) / (double)-segmentSize.DisplayedValue;
            }
            else
            {
                long bufferedOffset = Math.Max(0, offset - segmentSize.DisplayedValue * 2 * width);
                translateX = (bufferedOffset - offset) / (double)segmentSize.DisplayedValue;

                var request = new RenderingRequest(bufferedOffset, segmentSize, width * 5);

                var figures = dataAdapter.GetFiguresToDraw(
                    request.Offset,
                    request.Width,
                    segmentSize);

                _visualizer.Redraw(figures);

                _lastRenderingRequest = request;
            }

            var translateTransform = _chartArea.Canvas.RenderTransform as TranslateTransform;
            if (translateTransform == null)
                _chartArea.Canvas.RenderTransform = translateTransform = new TranslateTransform(0, 0);

            translateTransform.X = translateX;
            

            if (segmentSize.NeedToScale)
            {
                _chartArea.Canvas.LayoutTransform = new ScaleTransform(segmentSize.ScaleCoefficient, 1);
            }
            else
            {
                _chartArea.Canvas.LayoutTransform = null;
            }
        }
        
        
    }
}
