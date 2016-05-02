using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;

namespace IoTHubPiSense
{
    public delegate void MeasureMethod();

    public sealed class Telemetry
    {
        Timer timer;
        bool publishing = false;
        MeasureMethod measureMethod;
        static int msgCount = 0;

        public string Geo { get; set; }
        public string Celsius { get; set; }
        public string Humidity { get; set; }
        public string HPa { get; set; }
        public string Light { get; set; }
        public string Dev { get; set; }
        public int Id { get; set; }
        public int Cadence { get; set; } = 60000;
        public int Exceptions { get; set; }


        public Telemetry(string geo, string deviceId, MeasureMethod measureMethod) {
            this.Geo = geo;
            this.Dev = deviceId;
            this.measureMethod = measureMethod;
            timer = new Timer(ActionTimer, null, 1000, Cadence);
        }

        public byte[] ToJson(double temperature, double light, double hpa, double humidity) {
            Celsius = RoundMeasurement(temperature, 2).ToString();
            Light = RoundMeasurement(light, 2).ToString();
            HPa = RoundMeasurement(hpa, 0).ToString();
            Humidity = RoundMeasurement(humidity, 2).ToString();
            Id = ++msgCount;
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }

        private string RoundMeasurement(double value, int places) {
            return Math.Round(value, places).ToString();
        }

        void ActionTimer(object state) {
            if (!publishing) {
                publishing = true;
                measureMethod();
                publishing = false;
            }
        }

        public bool SetCadence(string cmd) {
            ushort newCadence = 0;
            if (ushort.TryParse(cmd, out newCadence)) {
                if (newCadence > 0) {
                    Cadence = newCadence * 1000;
                    timer.Change(0, Cadence);
                }
                return true;
            }
            return false;
        }




        //DateTime CorrectedTime() { // useful for locations particularly conferences with Raspberry Pi failes to sync time
        //    try {
        //        if (NtpInitalised) { return CorrectedUtcTime; }

        //        NtpClient ntp = new NtpClient();

        //        var time = ntp.GetAsync("au.pool.ntp.org").Result;
        //        utcOffset = DateTime.UtcNow.Subtract(((DateTime)time).ToUniversalTime());

        //        NtpInitalised = true;
        //    }
        //    catch { }

        //    return CorrectedUtcTime;
        //}




        //public Telemetry(string geo, string deviceId) {
        //    this.Geo = geo;
        //    this.Dev = deviceId;
        //}

        //public string Geo { get; set; }
        //public string Celsius { get; set; }
        //public string Humidity { get; set; }
        //public string HPa { get; set; }
        //public string Light { get; set; }
        //public string Utc { get; set; }
        //public string Dev { get; set; }

        //public byte[] ToJson(double temperature, double light, double hpa, double humidity) {
        //    Celsius = RoundMeasurement(temperature, 2).ToString();
        //    Light = RoundMeasurement(light, 2).ToString();
        //    HPa = RoundMeasurement(hpa, 0).ToString();
        //    Humidity = RoundMeasurement(humidity, 2).ToString();
        //    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        //}




        //public Telemetry(string guid, string measureName, string unitofmeasure) {
        //    this.guid = guid;
        //    this.measurename = measureName;
        //    this.unitofmeasure = unitofmeasure;
        //}



        //public string location => "USA";
        //public string organisation => "Fabrikam";
        //public string guid { get; set; }
        //public string measurename { get; set; }
        //public string unitofmeasure { get; set; }
        //public string value { get; set; }
        //public string timecreated { get; set; }
        //public int Id { get; set; }


        //public byte[] ToJson(double measurement) {
        //    value = RoundMeasurement(measurement, 2).ToString();
        //    //     timecreated = CorrectedTime().ToString("o");
        //    Id = ++msgCount;
        //    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        //}



    }
}
