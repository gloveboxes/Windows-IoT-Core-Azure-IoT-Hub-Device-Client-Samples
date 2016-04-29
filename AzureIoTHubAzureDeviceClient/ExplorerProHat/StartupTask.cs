using Glovebox.Graphics.Components;
using Glovebox.Graphics.Drivers;
using Glovebox.IoT.Devices.Converters;
using Glovebox.IoT.Devices.HATs;
using Glovebox.IoT.Devices.Sensors;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using static Glovebox.IoT.Devices.HATs.ExplorerHatPro;

namespace IoTHubExplorerProHat
{
    public sealed class StartupTask : IBackgroundTask
    {
        private DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPi3DG;SharedAccessKey=Y7KaNlIPwYOf7S70Gc/zeo0pOFcQww5OO/hZ7uAiEh0=");

        BackgroundTaskDeferral deferral;

        ExplorerHatPro hat = new ExplorerHatPro(ADS1015.Gain.Volt5);
        BME280 bme280 = new BME280();
        LED8x8Matrix matrix = new LED8x8Matrix(new Ht16K33());

        Telemetry telemetry;

        public void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();

            telemetry = new Telemetry("Sydney", "RPi3DG");

            ReceiveC2dAsync(deviceClient);

            var result = Task.Run(async () => {
                while (true) {
                    try {
                        matrix.DrawSymbol(Glovebox.Graphics.Grid.Grid8x8.Symbols.HourGlass);
                        matrix.FrameDraw();

                        var content = new Message(telemetry.ToJson(bme280.Temperature.DegreesCelsius, hat.AnalogRead(ExplorerHatPro.AnalogPin.Ain2).ReadRatio(), bme280.Pressure.Hectopascals, bme280.Humidity));
                        await deviceClient.SendEventAsync(content);

                        matrix.FrameClear();
                        matrix.FrameDraw();

                        await Task.Delay(20000); // don't leave this running for too long at this rate as you'll quickly consume your free daily Iot Hub Message limit
                    }
                    catch { hat.Light(Colour.Red).On(); }
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

                switch (command) {
                    //case "RED":
                    //    hat.Light(Colour.Red).On();
                    //    break;
                    case "GREEN":
                        hat.Light(Colour.Green).On();
                        break;
                    case "BLUE":
                        hat.Light(Colour.Blue).On();
                        break;
                    case "YELLOW":
                        hat.Light(Colour.Yellow).On();
                        break;
                    case "OFF":
                        for (int l = 0; l < hat.ColourCount; l++) {
                            hat.Light((Colour)l).Off();
                        }
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unrecognized command: {0}", command);
                        break;
                }
            }
        }
    }
}
