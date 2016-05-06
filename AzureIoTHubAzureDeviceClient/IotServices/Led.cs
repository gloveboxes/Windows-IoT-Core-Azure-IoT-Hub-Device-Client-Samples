using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotServices
{
    
    public class Led
    {
        ObservableQueue<String> q;

        public Led(ObservableQueue<String> q) {
            this.q = q;
            q.Dequeued += Q_Dequeued;
        }

        private void Q_Dequeued(object sender, ItemEventArgs<string> e) {
            Debug.WriteLine($"Led: {e.Item}");
        }
    }
}
