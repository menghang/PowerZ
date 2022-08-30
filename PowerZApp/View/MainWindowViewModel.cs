using System.Collections.Generic;
using System.Collections.ObjectModel;
using OxyPlot;
using PowerZApp.USB;

namespace PowerZApp.View
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private double voltage = 0;
        public double Voltage
        {
            get => this.voltage;
            set => SetProperty(ref this.voltage, value, nameof(this.DispVoltage));
        }
        public string DispVoltage => this.voltage.ToString("0.000") + "V";

        private double current = 0;
        public double Current
        {
            get => this.current;
            set => SetProperty(ref this.current, value, nameof(this.DispCurrent));
        }
        public string DispCurrent => this.current.ToString("0.0000") + "A";

        private double power = 0;
        public double Power
        {
            get => this.voltage;
            set => SetProperty(ref this.power, value, nameof(this.DispPower));
        }
        public string DispPower => this.power.ToString("0.000") + "W";

        public ObservableCollection<string> Devices { get; private set; } = new ObservableCollection<string>();
        private string selectedDevice = null;
        public string SelectedDevice
        {
            get => this.selectedDevice;
            set => SetProperty(ref this.selectedDevice, value);
        }

        public List<string> SampleRates { get; private set; } = new List<string>() { "10Hz", "50Hz", "100Hz" };
        private string selectedSampleRate = "10Hz";
        public string SelectedSampleRate
        {
            get => this.selectedSampleRate;
            set => SetProperty(ref this.selectedSampleRate, value);
        }

        private bool enableAutoRefresh = true;
        public bool EnableAutoRefresh
        {
            get => this.enableAutoRefresh;
            set => SetProperty(ref this.enableAutoRefresh, value);
        }

        private string model = null;
        public string Model
        {
            get => this.model;
            set => SetProperty(ref this.model, value);
        }

        private string savePath = null;
        public string SavePath
        {
            get => this.savePath;
            set => SetProperty(ref this.savePath, value);
        }

        private bool running = false;
        public bool Running
        {
            get => this.running;
            set
            {
                this.running = value;
                OnPropertyChanged(nameof(this.EnableStart));
                OnPropertyChanged(nameof(this.EnableStop));
            }
        }
        public bool EnableStart
        {
            get => !this.running;
        }
        public bool EnableStop
        {
            get => this.running;
        }

        public List<DataPoint> VSeries { get; private set; } = new List<DataPoint>();
        public List<DataPoint> CSeries { get; private set; } = new List<DataPoint>();
    }
}
