using EventsProcessingAPI;
using System.Windows;
using System.Windows.Controls;

namespace EventsChart
{
    interface IChartArea
    {
        Canvas Canvas { get; }
        void AddToView(FrameworkElement element);
        BucketContainer BucketContainer { get; }
        long Offset { get; }
        long SegmentSize { get; }
        int Width { get; }
        int Height { get; }
    }
}
