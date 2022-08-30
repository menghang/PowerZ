using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using PowerZApp.USB;

namespace PowerZApp.PowerZ
{
    class PowerZDevice
    {
        private const ushort ProductId = 0x5750;
        private const ushort VendorId = 0x0483;
        private readonly byte[] Cmd12 = { 0x55, 0x03, 0x10, 0x02, 0x6a, 0x6d, 0xe9 };
        private readonly byte[] Cmd3 = { 0x55, 0x03, 0x10, 0x03, 0x6b, 0xac, 0x29 };
        private readonly byte[] Cmd4 = { 0x55, 0x03, 0x10, 0x04, 0x6c, 0xed, 0xeb };
        private readonly byte[] Cmd5 = { 0x55, 0x05, 0x22, 0x80, 0xf1, 0x00, 0xed, 0x8e, 0x1e };
        private readonly byte[] CmdMassRead = { 0x55, 0x05, 0x22, 0x05, 0x0b, 0x00, 0x8c, 0xdd, 0x57 };
        private readonly byte[] CmdSingleRead = { 0x55, 0x05, 0x22, 0x05, 0x0b, 0x01, 0x1c, 0x8d };
        private readonly byte[] Cmd1Hz = { 0x55, 0x06, 0x31, 0x1a, 0xb1, 0x01, 0x00, 0x58, 0x35, 08 };
        private readonly byte[] Cmd10Hz = { 0x55, 0x06, 0x31, 0x1a, 0xb1, 0x01, 0x01, 0x59, 0xf4, 0xc8 };
        private readonly byte[] Cmd50Hz = { 0x55, 0x06, 0x31, 0x1a, 0xb1, 0x01, 0x02, 0x5a, 0xb4, 0xc9 };
        private readonly byte[] Cmd100Hz = { 0x55, 0x06, 0x31, 0x1a, 0xb1, 0x01, 0x03, 0x5b, 0x75, 0x09 };

        public enum SampleRates { SR1Hz, SR10Hz, SR50Hz, SR100Hz };

        private PowerZPoint point = null;
        private Dictionary<string, HidDevice> devices = null;
        private HidDevice device = null;
        private readonly Hid hid;
        private System.Timers.Timer timerWrite;

        public PowerZDevice()
        {
            this.hid = new Hid();
        }

        public PowerZPoint GetLatestPoint()
        {
            return this.point;
        }

        public bool SetSampleRate(SampleRates sr)
        {
            bool res;
            switch (sr)
            {
                case SampleRates.SR1Hz:
                    res = this.hid.Write(this.Cmd1Hz) == Hid.HID_RETURN.SUCCESS;
                    Thread.Sleep(5);
                    break;
                case SampleRates.SR10Hz:
                    res = this.hid.Write(this.Cmd10Hz) == Hid.HID_RETURN.SUCCESS;
                    Thread.Sleep(5);
                    break;
                case SampleRates.SR50Hz:
                    res = this.hid.Write(this.Cmd50Hz) == Hid.HID_RETURN.SUCCESS;
                    Thread.Sleep(5);
                    break;
                case SampleRates.SR100Hz:
                    res = this.hid.Write(this.Cmd100Hz) == Hid.HID_RETURN.SUCCESS;
                    Thread.Sleep(5);
                    break;
                default:
                    res = false;
                    break;
            }
            return res;
        }

        public bool UpdateDeviceList()
        {
            Collection<HidDevice> devs = new();
            Hid.GetHidDeviceList(devs, VendorId, ProductId);
            this.devices = new Dictionary<string, HidDevice>();
            if (devs != null && devs.Count > 0)
            {
                foreach (HidDevice d in devs)
                {
                    this.devices.Add(d.Sn, d);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public Collection<string> GetDeviceSnList()
        {
            Collection<string> snList = new();
            if (this.devices != null && this.devices.Count > 0)
            {
                foreach (string k in this.devices.Keys)
                {
                    snList.Add(k);
                }
            }
            return snList;
        }

        public bool SetDeviceBySn(string sn)
        {
            if (this.devices != null)
            {
                return this.devices.TryGetValue(sn, out this.device);
            }
            else
            {
                this.device = null;
                return false;
            }
        }

        public bool StartCapture()
        {
            if (this.device == null)
            {
                return false;
            }
            this.hid.SetDevice(this.device);
            this.hid.DeviceRemoved += Hid_DeviceRemoved;
            this.hid.ReadCompleted += Hid_ReadCompleted;
            WriteConfigs();
            _ = this.hid.ReadASync();
            this.timerWrite = new System.Timers.Timer(500);
            this.timerWrite.AutoReset = true;
            this.timerWrite.Elapsed += TimerWrite_Elapsed;
            this.timerWrite.Start();
            return true;
        }

        private bool WriteConfigs()
        {
            if (this.hid.Write(this.Cmd12) != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            Thread.Sleep(5);
            if (this.hid.Write(this.Cmd12) != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            Thread.Sleep(5);
            if (this.hid.Write(this.Cmd3) != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            Thread.Sleep(5);
            if (this.hid.Write(this.Cmd4) != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            Thread.Sleep(5);
            if (this.hid.Write(this.Cmd5) != Hid.HID_RETURN.SUCCESS)
            {
                return false;
            }
            Thread.Sleep(5);
            if (!SetSampleRate(SampleRates.SR10Hz))
            {
                return false;
            }
            return true;

        }

        private void TimerWrite_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.hid.Write(this.CmdMassRead) != Hid.HID_RETURN.SUCCESS)
            {

            }
        }

        public delegate void DataUpdatedHandler(object sender, PowerZDataEventArgs e);
        public event DataUpdatedHandler DataUpdated;

        public class PowerZDataEventArgs : EventArgs
        {
            public PowerZPoint Point;
            public PowerZDataEventArgs(PowerZPoint p)
            {
                this.Point = p;
            }
        }

        private void Hid_ReadCompleted(object sender, Hid.HidDataEventArgs e)
        {
            PowerZPoint p = PowerZPoint.Parse(e.data);
            if (p != null)
            {
                this.point = p;
                DataUpdated(this, new PowerZDataEventArgs(p));
            }
        }

        private void Hid_DeviceRemoved(object sender, EventArgs e)
        {
            StopCapture();
        }

        public void StopCapture()
        {
            if (this.timerWrite != null)
            {
                this.timerWrite.Elapsed -= TimerWrite_Elapsed;
                this.timerWrite.Close();
            }
            this.hid.DeviceRemoved -= Hid_DeviceRemoved;
            this.hid.ReadCompleted -= Hid_ReadCompleted;
            this.hid.CloseDevice();
        }
    }
}
