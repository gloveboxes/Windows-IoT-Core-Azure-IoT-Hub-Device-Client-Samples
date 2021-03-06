﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IotServices
{
    public delegate void MeasureMethod();
    public class Scheduler
    {
        //Timer timer;
        //bool publishing = false;

        MeasureMethod measureMethod;
        int sampleRateInSeconds = 60;  // defaults to sample every 60 seconds

        public int SampleRateInSeconds {
            get { return sampleRateInSeconds; }
            set {
                sampleRateInSeconds = value > 0 ? value : sampleRateInSeconds;
                //timer?.Change(0, sampleRateInSeconds * 1000);
            }
        }

        public Scheduler(MeasureMethod measureMethod, int sampleRateInSeconds) {
            if (measureMethod == null) { return; }

            this.measureMethod = measureMethod;
            this.sampleRateInSeconds = sampleRateInSeconds;

            //timer = new Timer(ActionTimer, null, 0, SampleRateInSeconds * 1000);

            if (SampleRateInSeconds > 0) {
                Task.Run(()=> Measure()); }
        }

        //void ActionTimer(object state) {
        //    if (!publishing) {
        //        publishing = true;
        //        measureMethod();
        //        publishing = false;
        //    }
        //}

        public bool SetSampleRateInSeconds(string cmd)
        {
            ushort newSampleRateInSeconds = 0;
            if (ushort.TryParse(cmd, out newSampleRateInSeconds))
            {
                SampleRateInSeconds = newSampleRateInSeconds;
                return true;
            }
            return false;
        }

        async void Measure()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(SampleRateInSeconds * 1000);
                    measureMethod();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
