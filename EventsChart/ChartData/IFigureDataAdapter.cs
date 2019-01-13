using EventsChart.Drawing;
using EventsProcessingAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsChart.ChartData
{
    interface IFigureDataAdapter
    {
        IEnumerable<IFigure> GetFiguresToDraw(long offset, long width, SegmentSize segmentSize);
    }
}
