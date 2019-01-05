using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EventsChart
{
    internal class ChartUpdater
    {
        private readonly Visualizer _visualizer;
        private readonly IChartArea _chartArea;

        public ChartUpdater(IChartArea chartArea)
        {
            _chartArea = chartArea;
            _visualizer = new Visualizer(chartArea);
            
        }


        public async Task Run()
        {
            var wpfChart = (EventsChartArea)_chartArea;

            if (!wpfChart.IsVisible || !wpfChart.IsLoaded)
                return;

            await _visualizer.Redraw();
        }
        
        
    }
}
