using System;
using OxyPlot.Axes;

namespace PowerZApp.PowerZ
{
    class PowerZPoint
    {
        public double Current { get; private set; }
        public double Voltage { get; private set; }
        public double Power { get; private set; }
        public DateTime Time { get; private set; }

        public double Time2 { get => DateTimeAxis.ToDouble(this.Time); }

        public static PowerZPoint Parse(byte[] data)
        {
            PowerZPoint point = null;
            if (data != null)
            {
                if (data[0] == 0xAA && data[1] == 0x25 && data[2] == 0x62
                    && data[3] == 0x05 && data[4] == 0x0B && data[5] == 0x01)
                {
                    point = new PowerZPoint();
                    point.Voltage = BitConverter.ToInt32(data, 6) / 1000000.0;
                    point.Current = Math.Abs(BitConverter.ToInt32(data, 10)) / 1000000.0;
                    point.Power = BitConverter.ToInt32(data, 14) / 1000000.0;
                    point.Time = DateTime.Now;
                }
            }
            return point;
        }
    }
}
