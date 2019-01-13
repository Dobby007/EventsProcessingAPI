using EventsChart.ChartData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace EventsChart
{
    internal class ChartUpdater
    {
        private readonly Visualizer _visualizer;
        private readonly IChartArea _chartArea;
        private readonly IFigureDataAdapterFactory _dataAdapterFactory;

        public ChartUpdater(IChartArea chartArea, IFigureDataAdapterFactory dataAdapterFactory)
        {
            _chartArea = chartArea;
            _visualizer = new Visualizer(chartArea);
            _dataAdapterFactory = dataAdapterFactory;
        }


        public async Task Run()
        {          
            var segmentSize = _chartArea.SegmentSize;

            var dataAdapter = _dataAdapterFactory.Get();
            if (dataAdapter == null)
                return;

            var figures = dataAdapter.GetFiguresToDraw(_chartArea.Offset, _chartArea.Width, segmentSize);
            _visualizer.Redraw(figures);

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
