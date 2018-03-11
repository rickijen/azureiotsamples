using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace CreateDeviceIdentity
{
    class Program
    {
        static RegistryManager registryManager;
        static string connectionString;
        static string deviceId;

        private static async Task AddDeviceAsync(string deviceId)
        {
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
            if (string.IsNullOrWhiteSpace(connectionString) && (args.Length < 2))
            {
                Console.WriteLine("CreateDeviceIdentity <connectionString> <deviceId>");
                return;
            }

            connectionString = args[0];
            deviceId = args[1];

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            AddDeviceAsync(deviceId).Wait();
            Console.ReadLine();
        }
    }
}
