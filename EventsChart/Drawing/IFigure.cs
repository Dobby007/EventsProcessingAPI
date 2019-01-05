using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EventsChart.Drawing
{
    /// <summary>
    /// Bridge pattern (Abstraction)
    /// </summary>
    interface IFigure
    {
        void Draw(IDrawingContext context);
    }
}
