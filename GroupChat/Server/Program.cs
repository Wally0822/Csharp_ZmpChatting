using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;

namespace Server;

public class ZmqServerManager
{
    private static readonly ConcurrentDictionary<string, NetMQFrame> concurrentDictionary = new();
    static readonly ConcurrentDictionary<string, NetMQFrame> clients = concurrentDictionary;

    public static void Main()
    {
        using var server = new RouterSocket();
        server.Bind("tcp://*:5555");

        while (true)
        {
            var clientMessage = server.ReceiveMultipartMessage();

            if (clientMessage.FrameCount < 2)
            {
                continue;
            }

            var clientAddressFrame = clientMessage[0];
            var clientMessageFrame = clientMessage[1];

            var clientAddress = clientAddressFrame.ConvertToString();

            clients.AddOrUpdate(clientAddress, clientAddressFrame, (s, frame) => clientAddressFrame);

            foreach (var client in clients.Values)
            {
                if (client.ToString() != clientAddress)
                {
                    var messageToOtherClient = new NetMQMessage();
                    messageToOtherClient.Append(client);
                    messageToOtherClient.Append(clientMessageFrame);
                    server.SendMultipartMessage(messageToOtherClient);
                }
            }

            Console.WriteLine($"Received: {clientMessageFrame.ConvertToString()}");
        }
    }
}