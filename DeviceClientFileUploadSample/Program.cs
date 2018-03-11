// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace DeviceClientFileUploadSample
{
    class Program
    {
        // String containing Hostname, Device Id & Device Key in one of the following formats:
        //  "HostName=<iothub_host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"
        //  "HostName=<iothub_host_name>;CredentialType=SharedAccessSignature;DeviceId=<device_id>;SharedAccessSignature=SharedAccessSignature sr=<iot_host>/devices/<device_id>&sig=<token>&se=<expiry_time>";
        static string DeviceConnectionString;
        static string FilePath;

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(DeviceConnectionString) && (args.Length < 2))
            {
                Console.WriteLine("DeviceClientFileUploadSample <DeviceConnectionString> <FilePath>");
                return;
            }

            DeviceConnectionString = args[0];
            FilePath = args[1];

            try
            {
                SendToBlobSample(DeviceConnectionString, FilePath).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}\n", ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message + "\n");
                }
            }
        }

        static async Task SendToBlobSample(string DeviceConnectionString, string FilePath)
        {
            var deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Http1);
            var fileStreamSource = new FileStream(FilePath, FileMode.Open);
            var fileName = Path.GetFileName(fileStreamSource.Name);

            Console.WriteLine("Uploading File: {0}", fileName);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            await deviceClient.UploadToBlobAsync(fileName, fileStreamSource).ConfigureAwait(false);
            watch.Stop();

            Console.WriteLine("Time to upload file: {0}ms\n", watch.ElapsedMilliseconds);
        }
    }
}
