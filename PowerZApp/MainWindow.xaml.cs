using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Wpf;
using PowerZApp.PowerZ;
using PowerZApp.View;

namespace PowerZApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel view;
        private readonly PowerZDevice device;
        private readonly Timer dispTimer;
        private bool enableDispUpdate = false;

        public MainWindow()
        {
            InitializeComponent();
            this.view = new MainWindowViewModel();
            this.DataContext = this.view;

            this.device = new PowerZDevice();
            this.device.DataUpdated += Device_DataUpdated;

            this.dispTimer = new Timer(1000);
            this.dispTimer.AutoReset = true;
            this.dispTimer.Elapsed += DispTimer_Elapsed;
            this.dispTimer.Start();
        }

        private void Device_DataUpdated(object sender, PowerZDevice.PowerZDataEventArgs e)
        {
            if (this.view.Running)
            {
                var p = e.Point;
                this.view.VSeries.Add(new DataPoint(p.Time2, p.Voltage));
                this.view.CSeries.Add(new DataPoint(p.Time2, p.Current));
            }
        }

        private void DispTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.enableDispUpdate)
            {
                var p = this.device.GetLatestPoint();
                if (p != null)
                {
                    this.view.Current = p.Current;
                    this.view.Voltage = p.Voltage;
                    this.view.Power = p.Power;
                }
                if (this.view.Running)
                {

                    this.plot.Dispatcher.BeginInvoke(new Action(() => this.plot.InvalidatePlot()));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDeviceList();
            if (this.view.Devices.Count > 0)
            {
                this.view.SelectedDevice = this.view.Devices[0];
                this.device.SetDeviceBySn(this.view.SelectedDevice);
                this.device.StartCapture();
                this.enableDispUpdate = true;
            }
        }

        private void UpdateDeviceList()
        {
            this.view.Devices.Clear();
            device.UpdateDeviceList();
            var devs = device.GetDeviceSnList();
            foreach (var d in devs)
            {
                this.view.Devices.Add(d);
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.view.Running = true;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            this.view.Running = false;
        }

        private void ComboBoxSampleRates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            bool res = false;
            switch (this.view.SelectedSampleRate)
            {
                case "1Hz":
                    res = this.device.SetSampleRate(PowerZDevice.SampleRates.SR1Hz);
                    break;
                case "10Hz":
                    res = this.device.SetSampleRate(PowerZDevice.SampleRates.SR10Hz);
                    break;
                case "50Hz":
                    res = this.device.SetSampleRate(PowerZDevice.SampleRates.SR50Hz);
                    break;
                case "100Hz":
                    res = this.device.SetSampleRate(PowerZDevice.SampleRates.SR100Hz);
                    break;
            }
            Trace.WriteLine("SetSampleRate:" + res);
        }
    }
}
