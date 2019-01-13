using EventsProcessingAPI;
using EventsProcessingAPI.Common;
using System.Windows;
using System.Windows.Controls;

namespace EventsChart
{
    interface IChartArea
    {
        Canvas Canvas { get; }
        void AddToView(FrameworkElement element);
        long Offset { get; }
        SegmentSize SegmentSize { get; }
        int Width { get; }
        int Height { get; }
    }
}
