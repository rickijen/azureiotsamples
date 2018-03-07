using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace ReadFileUploadNotification
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString;

        private async static void ReceiveFileUploadNotificationAsync()
        {
            var notificationReceiver = serviceClient.GetFileNotificationReceiver();

            Console.WriteLine("\nReceiving file upload notification from service");
            while (true)
            {
                var fileUploadNotification = await notificationReceiver.ReceiveAsync();
                if (fileUploadNotification == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received file upload noticiation: {0}", string.Join(", ", fileUploadNotification.BlobName));
                Console.ResetColor();

                await notificationReceiver.CompleteAsync(fileUploadNotification);
            }
        }

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(connectionString) && (args.Length < 1))
            {
                Console.WriteLine("ReadFileUploadNotification <coonnection string>");
                return;
            }

            connectionString = args[0];
            Console.WriteLine("Receive file upload notifications\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ReceiveFileUploadNotificationAsync();
            Console.WriteLine("Press Enter to exit\n");
            Console.ReadLine();
        }
    }
}
