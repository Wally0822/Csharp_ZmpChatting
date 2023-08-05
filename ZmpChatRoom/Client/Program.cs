using NetMQ;
using NetMQ.Sockets;
using System.Text;

namespace Client;

public class ZmqClientManager
{
    public static void Main()
    {
        Console.Write($"Enter your nickname : ");
        var nickname = Console.ReadLine();

        if(string.IsNullOrWhiteSpace(nickname))
        {
            nickname = "DefaultNickname";
        }

        Console.Write("Enter room name: ");
        var roomName = Console.ReadLine();
        
        if(string.IsNullOrWhiteSpace(roomName))
        {
            roomName = "DefaultRoom";
        }

        using var client = new DealerSocket();
        client.Options.Identity = Encoding.UTF8.GetBytes(nickname);
        client.Connect("tcp://localhost:5555");

        _ = Task.Run(() =>
        {
            while (true)
            {
                var message = client.ReceiveFrameString();
                if (string.IsNullOrEmpty(message))
                {
                    break;
                }

                var messageParts = message.Split(':', 5);
                if (messageParts.Length > 2 && messageParts[3].Trim() == nickname)
                {
                    continue;
                }

                Console.WriteLine(message);
            }
        });

        string? dataToSend;

        while ((dataToSend = Console.ReadLine()) != "<EOF>")
        {
            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            client.SendMoreFrame($"{roomName}");
            client.SendFrame($"[{timeStamp}] : {nickname}: {dataToSend}");
        }
    }
}