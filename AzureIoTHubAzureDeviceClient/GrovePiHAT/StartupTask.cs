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


// GrovePi Samples https://github.com/DexterInd/GrovePi/tree/master/Software/CSharp/Samples

namespace GrovePiHAT
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral deferral;
        DeviceClient deviceClient = DeviceClient.CreateFromConnectionString("HostName=glovebox-iot-hub.azure-devices.net;DeviceId=RPiGrovePi;SharedAccessKey=W1THCWLeUmbqlmdnv2gtpcTuxmlsa+tPQbSae0fuxNc=");

        IDHTTemperatureAndHumiditySensor dht11 = DeviceFactory.Build.DHTTemperatureAndHumiditySensor(Pin.DigitalPin3, DHTModel.Dht11);
        ILightSensor light = DeviceFactory.Build.LightSensor(Pin.AnalogPin0);
        IRelay relay = DeviceFactory.Build.Relay(Pin.DigitalPin2);
        ILed publishLed = DeviceFactory.Build.Led(Pin.DigitalPin5);

        IoTHubCommand<String> iotHubCommand;
        Telemetry telemetry;

        public void Run(IBackgroundTaskInstance taskInstance) {
            deferral = taskInstance.GetDeferral();

            telemetry = new Telemetry("Sydney", "RPiGrovePi", Publish, 60);

            iotHubCommand = new IoTHubCommand<string>(deviceClient, telemetry);
            iotHubCommand.CommandReceived += IotHubCommand_CommandReceived;

        }

        private void IotHubCommand_CommandReceived(object sender, CommandEventArgs<string> e) {
            switch (e.Item.ToUpper()) {
                case "ON":
                    relay.ChangeState(SensorStatus.On);
                    break;
                case "OFF":
                    relay.ChangeState(SensorStatus.Off);
                    break;
                default:
                    break;
            }
        }

        async void Publish() {
            if (dht11 == null || deviceClient == null) { return; }

            double temperature, humidity;

            try {
                publishLed.ChangeState(SensorStatus.On);

                dht11.Measure();

                temperature = dht11.TemperatureInCelsius;
                humidity = dht11.Humidity;

                if (double.IsNaN(temperature) || double.IsNaN(humidity)) { return; }                

                var content = new Message(telemetry.ToJson(temperature, light.SensorValue() * 100 / 1023, 0, humidity));

                await deviceClient.SendEventAsync(content);
            }
            catch { telemetry.Exceptions++; }

            publishLed.ChangeState(SensorStatus.Off);
        }
    }
}
