﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DeviceClientTwinSample
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;

    class Program
    {
        // String containing Hostname, Device Id & Device Key in one of the following formats:
        //  "HostName=<iothub_host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"
        //  "HostName=<iothub_host_name>;CredentialType=SharedAccessSignature;DeviceId=<device_id>;SharedAccessSignature=SharedAccessSignature sr=<iot_host>/devices/<device_id>&sig=<token>&se=<expiry_time>";
        private static string DeviceConnectionString;

        private static DeviceClient Client = null;

        static void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            Console.WriteLine();
            Console.WriteLine("Connection Status Changed to {0}", status);
            Console.WriteLine("Connection Status Changed Reason is {0}", reason);
            Console.WriteLine();
        }

        private static async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("desired property change:");
            Console.WriteLine(JsonConvert.SerializeObject(desiredProperties));

            Console.WriteLine("Sending current time as reported property");
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.Now;

            await Client.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

        static void Main(string[] args)
        {
            TransportType transport = TransportType.Mqtt;

            if (args.Length == 2 && args[1].ToLower().Equals("amqp"))
            {
                transport = TransportType.Amqp;
            }

            DeviceConnectionString = args[0];

            try
            {
                Console.WriteLine("Connecting to hub");
                Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString, transport);
                Client.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
                Client.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null).Wait();

                Console.WriteLine("Retrieving twin");
                var twinTask = Client.GetTwinAsync();
                twinTask.Wait();
                var twin = twinTask.Result;

                Console.WriteLine("initial twin value received:");
                Console.WriteLine(JsonConvert.SerializeObject(twin));

                Console.WriteLine("Sending app start time as reported property");
                TwinCollection reportedProperties = new TwinCollection();
                reportedProperties["DateTimeLastAppLaunch"] = DateTime.Now;
                
                // To remove a property, simply set it to null.
                //reportedProperties["dddddddddd"] = null;
                //TwinCollection connectivity = new TwinCollection();
                //connectivity["type"] = "cellular";
                //reportedProperties["connectivity"] = connectivity;
                //TwinCollection firmware = new TwinCollection();
                //firmware["version"] = "1.0.9";
                //reportedProperties["firmware"] = firmware;

                Client.UpdateReportedPropertiesAsync(reportedProperties);
            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    Console.WriteLine();
                    Console.WriteLine("Error in sample: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error in sample: {0}", ex.Message);
            }
            Console.WriteLine("Waiting for Events.  Press enter to exit...");

            Console.ReadLine();
            Console.WriteLine("Exiting...");

            Client?.CloseAsync().Wait();

        }
    }
}
