﻿using EventsProcessingAPI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventsChartExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

       

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Файл с событиями|*.bin"
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                SelectiFilePath.Text = filename;
                LoadBtn.IsEnabled = true;
            }
        }

        private async void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var apiFacade = new ApiFacade();
                apiFacade.ProgressHandler = new Progress<int>(val =>
                {
                    LoadProgress.Value = val;
                });
                LoadProgress.Visibility = Visibility.Visible;
                LoadBtn.IsEnabled = false;

                var bucketContainer = await apiFacade.LoadEventsFromFileAsync(SelectiFilePath.Text, LoadStrategyType.LoadEventsForChart);

                var chartWindow = new ChartWindow(bucketContainer);
                chartWindow.Owner = this;
                Visibility = Visibility.Hidden;
            
                chartWindow.ShowDialog();
            }
            catch (Exception exc)
            {
                ShorExceptionMessageBox(exc);
            }

            Visibility = Visibility.Visible;
            ResetControls();

        }

        private void ResetControls()
        {
            LoadBtn.IsEnabled = true;
            LoadProgress.Visibility = Visibility.Collapsed;
            LoadProgress.Value = 0;

        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShorExceptionMessageBox(e.Exception);
            Visibility = Visibility.Visible;
            ResetControls();
            e.Handled = true;
        }

        private void ShorExceptionMessageBox(Exception exc)
        {
            string errorMessage = string.Format("An unhandled exception occurred: {0}", exc.ToString());
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

    }
}
