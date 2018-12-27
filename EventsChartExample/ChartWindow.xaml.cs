using EventsProcessingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EventsChartExample
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public BucketContainer BucketContainer { get; }
        public ChartWindow(BucketContainer bucketContainer)
        {
            BucketContainer = bucketContainer;
            InitializeComponent();
        }

        private void EventsChart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Debug.WriteLine("Mouse wheel delta: {0}", e.Delta);

            if (e.Delta > 0)
            {
                //EventsChart.ZoomIn();
            }
            else
            {
                //EventsChart.ZoomOut();
            }
        }
    }
}
