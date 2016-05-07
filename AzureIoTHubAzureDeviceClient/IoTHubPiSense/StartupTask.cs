using Emmellsoft.IoT.Rpi.SenseHat;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts.SingleColor;
using IotServices;
using Microsoft.Azure.Devices.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTHubPiSense
{
    public sealed class StartupTask : IBackgroundTask
    {
        private DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiSense;SharedAccessKey=AesJg+psAjbFHFgJ4BuqopBv8sIpb88q6ZdWMqVDqQw=");

        BackgroundTaskDeferral deferral;
        ISenseHat hat;
        ISenseHatDisplay display;
        Telemetry telemetry;
        TinyFont tinyFont = new TinyFont();
        Color statusColour = Colors.Blue;

        ObservableConcurrentQueue<String> q = new ObservableConcurrentQueue<string>();
        

        public async void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();
            telemetry = new Telemetry("Sydney", "RPiSense", Measure);
            hat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);
            display = hat.Display;

            q.Dequeued += Q_Dequeued;

            ReceiveC2dAsync(deviceClient);

            while (true) {
                q.Dequeue();
            }
        }

        private void Q_Dequeued(object sender, ItemEventArgs<string> e) {
            Debug.WriteLine(e.Item);
        }

        async void Measure() {
            try {
                display.Fill(statusColour);
                display.Update();

                hat.Sensors.HumiditySensor.Update();

                if (hat.Sensors.Temperature.HasValue && hat.Sensors.Humidity.HasValue) {
                    var content = new Message(telemetry.ToJson(hat.Sensors.Temperature.Value, 50, 1010, hat.Sensors.Humidity.Value));
                    await deviceClient.SendEventAsync(content);
                }

                display.Clear();

                if (telemetry.Exceptions > 0) {
                    tinyFont.Write(display, "E", Colors.Red);
                }

                display.Update();
            }
            catch { telemetry.Exceptions++; }
        }

        private async void ReceiveC2dAsync(DeviceClient deviceClient) {
            while (true) {
                try {
                    Message receivedMessage = await deviceClient.ReceiveAsync();
                    if (receivedMessage == null) {
                        await Task.Delay(2000);
                        continue;
                    }

                    await deviceClient.CompleteAsync(receivedMessage);
                    string command = Encoding.ASCII.GetString(receivedMessage.GetBytes()).ToUpper();

                    q.Enqueue(command);

                    if (telemetry.SetSampleRateInSeconds(command)) { continue; }

                    switch (command) {
                        case "RED":
                            statusColour = Colors.Red;
                            break;
                        case "GREEN":
                            statusColour = Colors.Green;
                            break;
                        case "BLUE":
                            statusColour = Colors.Blue;
                            break;
                        case "YELLOW":
                            statusColour = Colors.Yellow;
                            break;
                        default:
                            statusColour = Colors.Purple;
                            break;
                    }

                    display.Fill(statusColour);
                    display.Update();
                }
                catch { telemetry.Exceptions++; }
            }
        }
    }
}
