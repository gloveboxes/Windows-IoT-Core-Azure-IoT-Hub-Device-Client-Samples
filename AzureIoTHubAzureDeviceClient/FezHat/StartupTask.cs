
using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace IoTHubFezHat
{
    public sealed class StartupTask : IBackgroundTask
    {
        private DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiFez;SharedAccessKey=VHWMLDbUZ7EOsbeS5NfO560+xFjhrMYh5Q1Bga4wQHg=");

        BackgroundTaskDeferral deferral;
        FEZHAT hat;


        Telemetry telemetry;

        public async void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();
            hat = await FEZHAT.CreateAsync();

            telemetry = new IoTHubFezHat.Telemetry("Sydney", "RPiFez");

            ReceiveC2dAsync(deviceClient);

            var result = Task.Run(async () => {
                while (true) {
                    try {

                        hat.D3.Color = new FEZHAT.Color(0, 255, 0);

                        var content = new Message(telemetry.ToJson(hat.GetTemperature(), hat.GetLightLevel(), 0, 0));
                        await deviceClient.SendEventAsync(content);

                        hat.D3.TurnOff();

                        await Task.Delay(20000); // don't leave this running for too long at this rate as you'll quickly consume your free daily Iot Hub Message limit
                    }
                    catch {hat.D2.Color = new FEZHAT.Color(127, 0, 255); }  //purple http://rapidtables.com/web/color/RGB_Color.htm
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
                    case "RED":
                        hat.D2.Color = new FEZHAT.Color(255, 0, 0);
                        break;
                    case "GREEN":
                        hat.D2.Color = new FEZHAT.Color(0, 255, 0);
                        break;
                    case "BLUE":
                        hat.D2.Color = new FEZHAT.Color(0, 0, 255);
                        break;
                    case "YELLOW":
                        hat.D2.Color = new FEZHAT.Color(255, 255, 0);
                        break;
                    case "OFF":
                        hat.D2.TurnOff();
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unrecognized command: {0}", command);
                        break;
                }
            }
        }
    }
}
