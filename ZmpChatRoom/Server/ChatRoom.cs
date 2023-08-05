using NetMQ;
using System.Collections.Concurrent;

namespace Server;

public class ChatRoom
{
    public string Name { get; set; }
    public int MaxClinets { get; set; }

    public ConcurrentDictionary<string, NetMQFrame> Clients { get; set; }

    public ChatRoom(string name, int maxClients)
    {
        Name = name;
        MaxClinets = maxClients;
        Clients = new ConcurrentDictionary<string, NetMQFrame>();
    }
}