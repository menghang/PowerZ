namespace PowerZApp.USB
{
    internal class HidDevice
    {
        public string Path { get; private set; } = string.Empty;
        public string Sn { get; private set; } = string.Empty;
        public int OutputReportLength { get; private set; } = -1;
        public int InputReportLength { get; private set; } = -1;

        public HidDevice(string path, string sn, int inputReportLength, int outputReportLength)
        {
            this.Path = path;
            this.Sn = sn;
            this.InputReportLength = inputReportLength;
            this.OutputReportLength = outputReportLength;
        }
    }
}
