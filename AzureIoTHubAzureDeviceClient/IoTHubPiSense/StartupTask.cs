using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Microsoft.Azure.Devices.Client;
using Emmellsoft.IoT.Rpi.SenseHat;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTHubPiSense
{
    public sealed class StartupTask : IBackgroundTask
    {
        private DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiSC;SharedAccessKey=z5c+MtYY5zMy7wj3SDiRMpZC7W+UiOkaKTxh/5kP6+c=");

        BackgroundTaskDeferral deferral;

        ISenseHat hat;

        Telemetry telemetry;

        public async void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();


            telemetry = new Telemetry("Sydney", "RPiSC");


            hat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);

            //   ReceiveC2dAsync(deviceClient);

            var result = Task.Run(async () => {
                while (true) {
                    hat.Sensors.HumiditySensor.Update();

                    var content = new Message(telemetry.ToJson(0, 0, 0, 0));
                    await deviceClient.SendEventAsync(content);

                    await Task.Delay(20000); // don't leave this running for too long at this rate as you'll quickly consume your free daily Iot Hub Message limit
                }
            });
        }
    }
}
