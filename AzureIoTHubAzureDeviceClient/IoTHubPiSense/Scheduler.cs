//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace IoTHubPiSense
//{
//    public delegate void MeasureMethod();
//    public class Scheduler
//    {
        
//        Timer timer;
//        bool publishing = false;
//        MeasureMethod measureMethod;
//        int cadence = 60000;
//        public int Cadence {
//            get { return cadence; }
//            set {
//                cadence = value > 0 ? value : cadence;
//                timer?.Change(0, cadence);
//            }
//        }

//        public Scheduler(MeasureMethod measureMethod) {
//            this.measureMethod = measureMethod;
//            timer = new Timer(ActionTimer, null, 0, Cadence);
//        }

//        void ActionTimer(object state) {
//            if (!publishing) {
//                publishing = true;
//                measureMethod();
//                publishing = false;
//            }
//        }

//        public bool SetCadence(string cmd) {
//            ushort newCadence = 0;
//            if (ushort.TryParse(cmd, out newCadence)) {
//                if (newCadence > 0) {
//                    Cadence = newCadence * 1000;
//                }
//                return true;
//            }
//            return false;
//        }
//    }
//}
