using EventsProcessingAPI;
using EventsProcessingAPI.Common;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace EventsChart
{
    interface IChartArea : INotifyPropertyChanged
    {
        Canvas Canvas { get; }
        void AddUiElement(FrameworkElement element);
        void AddToArea(FrameworkElement element);
        long Offset { get; }
        SegmentSize SegmentSize { get; }
        int Width { get; }
        int Height { get; }
    }
}
