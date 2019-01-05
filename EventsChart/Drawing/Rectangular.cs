using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EventsChart.Drawing
{
    sealed class Rectangular : IFigure
    {
        private readonly Rect _rect;

        public Rectangular(Rect rect)
        {
            _rect = rect;
        }

        public void Draw(IDrawingContext context)
        {
            context.DrawRectangle(_rect);
        }
    }
}
