using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentity
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=rmtmonitor6d38c.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=mvdtgRmtWv5SmHGZoXjL/U4w9cNKZZ8389XeGhcXLd8=";

        private static async Task AddDeviceAsync()
        {
            string deviceId = "CoolSculptingDevice-002";
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }

        static void Main(string[] args)
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync().Wait();
            Console.ReadLine();
        }
    }
}
