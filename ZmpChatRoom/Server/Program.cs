using NetMQ;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Text;

namespace Server;

public class ZmqServerManager
{
    private static readonly ConcurrentDictionary<string, ChatRoom> chatRooms = new();

    public static void Main()
    {
        using var server = new RouterSocket();
        server.Bind("tcp://*:5555");

        while (true)
        {
            var clientMessage = server.ReceiveMultipartMessage();

            if (clientMessage.FrameCount < 3)
            {
                continue;
            }

            var clientAddressFrame = clientMessage[0];
            var roomNameFrame = clientMessage[1];
            var clientMessageFrame = clientMessage[2];

            var roomName = roomNameFrame.ConvertToString();
            var clientAddress = clientAddressFrame.ConvertToString();

            if (!chatRooms.ContainsKey(roomName))
            {
                var newRoom = new ChatRoom(roomName, 1);
                newRoom.Clients.TryAdd(clientAddress, clientAddressFrame);
                chatRooms.TryAdd(roomName, newRoom);
                Console.WriteLine($"Room [{roomName}] created with [{clientAddress}] as the first member.");
            }
            else
            {
                var room = chatRooms[roomName];

                if (room.Clients.Count < room.MaxClinets)
                {
                    room.Clients.TryAdd(clientAddress, clientAddressFrame);
                    Console.WriteLine($"{clientAddress} joined the room {roomName}.");
                }
                else
                {
                    string fullInfo = $"Room {roomName} is full. {clientAddress} cannot join.";
                    Console.WriteLine(fullInfo);

                    var messageToOtherClient = new NetMQMessage();
                    messageToOtherClient.Append(clientAddress);
                    messageToOtherClient.Append(clientMessageFrame);
                    messageToOtherClient.Append(Encoding.UTF8.GetBytes(fullInfo));
                    server.SendMultipartMessage(messageToOtherClient);

                    continue;
                }
            }
            foreach (var client in chatRooms[roomName].Clients.Values)
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