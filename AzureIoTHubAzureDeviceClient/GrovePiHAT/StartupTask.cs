using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using GrovePi.Sensors;
using GrovePi;
using IotServices;
using Microsoft.Azure.Devices.Client;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace GrovePiHAT
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiGrovePi;SharedAccessKey=W1THCWLeUmbqlmdnv2gtpcTuxmlsa+tPQbSae0fuxNc=");

        IDHTTemperatureAndHumiditySensor sensor = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin4, DHTModel.Dht11);
        ILightSensor light = DeviceFactory.Build.LightSensor(Pin.AnalogPin0);

        IoTHubCommand<String> iotHubCommand;
        Telemetry telemetry;

        public void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();

            telemetry = new Telemetry("Sydney", "RPiFez", Publish, 60);

            iotHubCommand = new IoTHubCommand<string>(deviceClient, telemetry);
            iotHubCommand.CommandReceived += IotHubCommand_CommandReceived;

        }

        private void IotHubCommand_CommandReceived(object sender, CommandEventArgs<string> e) {
            //  todo add something...
        }

        async void Publish() {
            if (sensor == null || deviceClient == null) { return; }

            try {
                sensor.Measure();

                var content = new Message(telemetry.ToJson(sensor.TemperatureInCelsius, light.SensorValue(), 0, sensor.Humidity));
                await deviceClient.SendEventAsync(content);
            }
            catch { telemetry.Exceptions++; }
        }
    }
}
