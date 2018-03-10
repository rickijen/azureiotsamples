using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace SendCloudToDevice
{
    class Program
    {
        static ServiceClient serviceClient;
        static string connectionString;
        static string deviceId;

        private async static Task SendCloudToDeviceMessageAsync(string deviceId)
        {
            var commandMessage = new Message(Encoding.ASCII.GetBytes("XXXXXXXXXX Cloud to device message XXXXXXXXXX"));
            commandMessage.Ack = DeliveryAcknowledgement.Full;
            await serviceClient.SendAsync(deviceId, commandMessage);
        }

        private async static void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();

            Console.WriteLine("\nReceiving c2d feedback from service");
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Received feedback: {0}", string.Join(", ", feedbackBatch.Records.Select(f => f.StatusCode)));
                Console.ResetColor();

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(connectionString) && (args.Length < 2))
            {
                Console.WriteLine("SendCloudToDevice <connectionString> <deviceId>");
                return;
            }

            connectionString = args[0];
            deviceId = args[1];

            Console.WriteLine("Send Cloud-to-Device message\n");
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            ReceiveFeedbackAsync();

            Console.WriteLine("Press any key to send a C2D message.");
            
            Console.ReadLine();
            SendCloudToDeviceMessageAsync(deviceId).Wait();
            Console.ReadLine();
        }
    }
}
