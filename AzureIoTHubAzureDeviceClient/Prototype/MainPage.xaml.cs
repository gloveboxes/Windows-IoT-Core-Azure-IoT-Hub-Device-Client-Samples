using IotServices;
using System;
using System.Diagnostics;
using System.Threading;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Prototype
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableConcurrentQueue<String> q = new ObservableConcurrentQueue<string>();
        Timer dataGeneratoreTimer;
        int QCount = 0;
        Led l1;
        Led l2;

        public MainPage()
        {
            this.InitializeComponent();

            l1 = new Led(q);
            l2 = new Led(q);

            dataGeneratoreTimer = new Timer(SampleData, null, 0, 200);

            

            q.Dequeued += Q_Dequeued;
        }

        private void Q_Dequeued(object sender, ItemEventArgs<string> e) {
            Debug.WriteLine($"Item {e.Item}, Queue Count {q.Count}");
         //   Task.Delay(100).Wait();  //simualate some workload
        }

     

        void SampleData(object state) {
            if (q.Count > 200) { return; }
      
                q.Enqueue($"Hello World {QCount++}");
          //      Debug.WriteLine($"Queued {QCount}");
        }
    }
}
