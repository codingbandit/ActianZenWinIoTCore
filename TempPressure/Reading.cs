using System;

namespace TempPressure
{
    public class Reading
    {
        public string DeviceName { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Altitude { get; set; }
        public DateTime ReadingTs { get; set; }
    }
}
