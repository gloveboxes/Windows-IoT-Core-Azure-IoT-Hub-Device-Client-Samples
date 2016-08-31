
using GHIElectronics.UWP.Shields;
using IotServices;
using Microsoft.Azure.Devices.Client;
using System;
using Windows.ApplicationModel.Background;

namespace IoTHubFezHat
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;

        DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiFez;SharedAccessKey=VHWMLDbUZ7EOsbeS5NfO560+xFjhrMYh5Q1Bga4wQHg=");
        IoTHubCommand<String> iotHubCommand;
        Telemetry telemetry;
        FEZHAT hat;

        public async void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();

            hat = await FEZHAT.CreateAsync();
            hat.D2.TurnOff();
            hat.D3.TurnOff();

            telemetry = new Telemetry("Sydney", Publish, 30);

            iotHubCommand = new IoTHubCommand<string>(deviceClient, telemetry);
            iotHubCommand.CommandReceived += Commanding_CommandReceived;
        }

        async void Publish() {
            if (hat == null || deviceClient == null) { return; }

            try {
                hat.D3.Color = new FEZHAT.Color(0, 255, 0);

                var content = new Message(telemetry.ToJson(hat.GetTemperature(), hat.GetLightLevel(), 0, 0));
                await deviceClient.SendEventAsync(content);

                hat.D3.TurnOff();
            }
            catch { telemetry.Exceptions++; }
        }

        private void Commanding_CommandReceived(object sender, CommandEventArgs<string> e) {
            switch (e.Item.ToUpper()) {
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
                    System.Diagnostics.Debug.WriteLine("Unrecognized command: {0}", e.Item);
                    break;
            }
        }
    }
}
