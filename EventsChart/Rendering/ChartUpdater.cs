using EventsChart.ChartData;
using EventsProcessingAPI.Common;
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
            SegmentSize segmentSize = _chartArea.SegmentSize;
            long offset = _chartArea.Offset;
            int width = _chartArea.Width;
            int height = _chartArea.Height;

            var dataAdapter = _dataAdapterFactory.Get();
            if (dataAdapter == null)
                return;

            double translateX = 0;
            if (_lastRenderingRequest.Covers(offset, segmentSize, width, height))
            {
                translateX = (offset - _lastRenderingRequest.Offset) / (double)-segmentSize.DisplayedValue;
            }
            else
            {
                // TODO: Определять буферную зону динамически в зависимости от текущего масштаба и расстоянием между начальным и конечным событием
                long bufferedOffset = Math.Max(0, offset - segmentSize.DisplayedValue * 2 * width);
                var request = new RenderingRequest(bufferedOffset, segmentSize, width * 5, height);

                translateX = (bufferedOffset - offset) / (double)segmentSize.DisplayedValue;

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
