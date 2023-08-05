using NetMQ;
using NetMQ.Sockets;

namespace Client;

public class ZmqClientManager
{
    public static void  Main()
    {
        Console.Write($"Enter you nickname: ");
        var nickname = Console.ReadLine();

        if(string.IsNullOrWhiteSpace(nickname))
        {
            nickname = "DefaultNickname";
        }

        // C# 8.0에서는 using 문법이 간소화되어 중괄호 범위를 사용하지 않아도 된다.
        // 간소화된 using 문법 사용
        using var client = new DealerSocket();
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

        while ((dataToSend = Console.ReadLine()) != "<EOF>")
        {
            client.SendFrame($"{nickname}: {dataToSend}");
        }
    }
}