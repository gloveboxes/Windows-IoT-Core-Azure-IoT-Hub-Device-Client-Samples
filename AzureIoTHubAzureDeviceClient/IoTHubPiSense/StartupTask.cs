using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Microsoft.Azure.Devices.Client;
using Emmellsoft.IoT.Rpi.SenseHat;
using System.Threading.Tasks;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts.SingleColor;
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


        public async void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();
            telemetry = new Telemetry("Sydney", "RPiSense");
            hat = await SenseHatFactory.GetSenseHat().ConfigureAwait(false);
            display = hat.Display;

            ReceiveC2dAsync(deviceClient);

            var result = Task.Run(async () => {
                while (true) {
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

                        await Task.Delay(telemetry.Cadence); // don't leave this running for too long at this rate as you'll quickly consume your free daily Iot Hub Message limit
                    }
                    catch { telemetry.Exceptions++; }
                }
            });
        }

        private async void ReceiveC2dAsync(DeviceClient deviceClient) {
            while (true) {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) {
                    await Task.Delay(2000);
                    continue;
                }

                await deviceClient.CompleteAsync(receivedMessage);
                string command = Encoding.ASCII.GetString(receivedMessage.GetBytes()).ToUpper();

                if (telemetry.SetCadence(command)) { continue; }

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
        }
    }
}
