using EventsChart.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsChart.ChartData
{
    interface IDataAdapter
    {
        IEnumerable<IFigure> GetFiguresToDraw(long start, long end, long segmentSize);
    }
}
