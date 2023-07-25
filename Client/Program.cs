using NetMQ;
using NetMQ.Sockets;

namespace Client;

public class ZmqClientManager
{
    public static void  Main()
    {
        Console.Write($"Enter you nickname: ");
        var nickname = Console.ReadLine();
        if(string.IsNullOrEmpty(nickname))
        {
            nickname = "DefaultNickname";
        }    

        using (var client = new DealerSocket())
        {
            client.Options.Identity = System.Text.Encoding.UTF8.GetBytes(nickname);
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

                    var messageParts = message.Split(':');
                    if (messageParts.Length > 1 && messageParts[0].Trim() == nickname)
                    {
                        continue;
                    }

                    Console.WriteLine(message);
                }
            });

            string? dataToSend;

            while((dataToSend = Console.ReadLine()) != "<EOF>")
            {
                client.SendFrame($"{nickname}: {dataToSend}");
            }
        }
    }
}