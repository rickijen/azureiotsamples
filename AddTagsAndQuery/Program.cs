using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace AddTagsAndQuery
{
    class Program
    {
        static RegistryManager registryManager;
        private static string connectionString;
        private static string deviceId;
        //static string connectionString = "HostName=rmtmonitor6d38c.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=mvdtgRmtWv5SmHGZoXjL/U4w9cNKZZ8389XeGhcXLd8=";
        public static async Task AddTagsAndQuery(string deviceId)
        {
            var twin = await registryManager.GetTwinAsync(deviceId);
            var patch =
                @"{
             tags: {
                 location: {
                     region: 'JP',
                     clinic: 'Slim Tokyo'
                 }
             }
         }";
            await registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);

            var query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.clinic = 'Slim Tokyo'", 100);
            var twinsInRedmond43 = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Slim Tokyo: {0}", string.Join(", ", twinsInRedmond43.Select(t => t.DeviceId)));

            query = registryManager.CreateQuery("SELECT * FROM devices WHERE tags.location.clinic = 'Slim Tokyo' AND properties.reported.connectivity.type = 'cellular'", 100);
            var twinsInRedmond43UsingCellular = await query.GetNextAsTwinAsync();
            Console.WriteLine("Devices in Slim Tokyo using cellular network: {0}", string.Join(", ", twinsInRedmond43UsingCellular.Select(t => t.DeviceId)));
        }

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(connectionString) && (args.Length < 2))
            {
                Console.WriteLine("AddTagsAndQuery <coonnection string> <device id>");
                return;
            }

            connectionString = args[0];
            deviceId = args[1];
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddTagsAndQuery(deviceId).Wait();
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
