using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri;
        static string deviceKey;
        static string deviceID;

        private static async void ReceiveC2dAsync()
        {
            Console.WriteLine("\nReceiving cloud to device messages from service");
            while (true)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(receivedMessage.GetBytes()));
                Console.ResetColor();

                await deviceClient.CompleteAsync(receivedMessage);
            }
        }
        private static async void SendDeviceToCloudMessagesAsync(string deviceID)
        {
            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = deviceID,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Route messages to a specific endpoint based on query string defined.
                // E.g. use query string RICK='JEN' and route messages to a specific storage container.
                // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-query-language#device-to-cloud-message-routes-query-expressions

                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                message.Properties.Add("RICK", "JEN");

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private static async void SendToBlobAsync()
        {
            string fileName = "image.jpg";
            Console.WriteLine("Uploading file: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(@"image.jpg", FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(fileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(iotHubUri) && (args.Length < 3))
            {
                Console.WriteLine("SimulatedDevice <iotHubUri> <deviceKey> <deviceId>");
                return;
            }

            iotHubUri = args[0];
            deviceKey = args[1];
            deviceID = args[2];

            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceID, deviceKey), TransportType.Mqtt);
            deviceClient.ProductInfo = "Product Info";
            
            // Send D2C Messages
            SendDeviceToCloudMessagesAsync(deviceID);
            // Receive C2D Messages 
            ReceiveC2dAsync();
            // File upload
            SendToBlobAsync();

            Console.ReadLine();
        }
    }
}
