
using System;
using System.Net.Mime;
using System.Reflection;

using static ClientFramework.NetworkEvents;

namespace ClientFramework;
public class Program
{

    public static void OnClientConnected(object sender, OnClientConnectEvent eventData)
    {
        if (eventData.ClientID == Network.Client.ID)
        {
            Console.WriteLine($"*EVENT* YOU CONNECTED! ({eventData.UserName} ID:{eventData.ClientID} SUCCESS:{eventData.Success})");
            return;
        }
        Console.WriteLine($"*EVENT* CLIENT CONNECTED! ({eventData.UserName} ID:{eventData.ClientID})");
    }
    public static void OnClientDisconnected(object sender, OnClientDisconnectEvent eventData)
    {
        if (eventData.ClientID == Network.Client.ID)
        {
            Console.WriteLine($"*EVENT* YOU DISCONNECTED! ({eventData.UserName} ID:{eventData.ClientID} SUCCESS:{eventData.Success})");
            return;
        }
        Console.WriteLine($"*EVENT* CLIENT DISCONNECTED! ({eventData.UserName} ID:{eventData.ClientID} SUCCESS:{eventData.Success})");
    }
    public static void OnServerShutdown(object sender, OnServerShutdownEvent eventData)
    {
        Console.WriteLine($"*EVENT* SERVER STOPPED! SUCCESS:{eventData.Success}");
    }
    public static void OnMessageSent(object sender, OnMessageSentEvent eventData)
    {
        Console.WriteLine($"*EVENT* MSG SENT: {eventData.Message?.MethodName}");
    }
    public static void OnMessageReceived(object sender, OnMessageReceivedEvent eventData)
    {
        Console.WriteLine($"*EVENT* MSG RECEIVED: {eventData.Message?.MethodName}");
    }
    public static void OnHandShakeStart(object sender, OnHandShakeStartEvent eventData)
    {
        Console.WriteLine($"*EVENT* HANDSHAKE STARTED: version:{eventData.ClientVersion}, username:{eventData.UserName}");
    }
    public static void OnHandShakeEnd(object sender, OnHandShakeEndEvent eventData)
    {
        Console.WriteLine($"*EVENT* HANDSHAKE ENDED: Success:{eventData.Success}, Code:{eventData.ErrorCode}");
        //StatusCode: 0 = not defined, 1 = server issue, not defined, 2 = version mismatch, 3 = username already in use
    }
    public static void Main(string[] args)
    {
        Logger.Debug = true;

        NetworkEvents.Listener.ClientConnected += OnClientConnected;
        NetworkEvents.Listener.ClientDisconnect += OnClientDisconnected;
        NetworkEvents.Listener.ServerShutdown += OnServerShutdown;
        NetworkEvents.Listener.MessageSent += OnMessageSent;
        NetworkEvents.Listener.MessageReceived += OnMessageReceived;
        NetworkEvents.Listener.HandshakeStart += OnHandShakeStart;
        NetworkEvents.Listener.HandshakeEnd += OnHandShakeEnd;

        Network.RegisterMethod(typeof(ClientMethods));

        Console.Title = "CLIENT";
        Console.Clear();
        Console.WriteLine("Type 'help' for commands!");

        while (true)
        {
            Console.WriteLine();
            string? command = Console.ReadLine();
            command = command?.ToLower();

            try
            {
                switch (command)
                {
                    case "help":
                        Commands.Help();
                        break;
                    case "toggledebug":
                        Logger.Debug = !Logger.Debug;
                        Console.WriteLine($"Debug is now: {Logger.Debug}");
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    case "exit":
                        Environment.Exit(0);
                        break;
                    case "connect":
                        Console.WriteLine("Enter IP adress:");
                        string? ip = Console.ReadLine();
                        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";
                        Console.WriteLine("Enter Port");
                        string? port = Console.ReadLine();
                        if (string.IsNullOrEmpty(port)) port = "5001";
                        Console.WriteLine("Username:");
                        string? name = Console.ReadLine();
                        if (string.IsNullOrEmpty(name))
                        {
                            Random rd = new Random();
                            name = ("RANDOMUSER" + rd.Next(1, 10).ToString());
                        }
                        Network.Connect(ip, Int32.Parse(port), name);
                        break;
                    case "disconnect":
                        Network.Disconnect();
                        break;
                    case "users":
                        Commands.UserList();
                        break;
                    case "senddata":
                        Commands.SendData();
                        break;
                    case "clientmethods":
                        Commands.GetClientMethods();
                        break;
                    case "servermethods":
                        Commands.GetServerMethods();
                        break;
                    case "requestdata":
                        Commands.RequestData();
                        break;
                    case "requestdatatype":
                        Commands.RequestDataType();
                        break;
                    case "status":
                        Console.WriteLine(Network.IsConnected() ? "Connected to server! ID:" + Network.Client.ID.ToString() : "NOT connected to server!");
                        break;
                    default:
                        Console.WriteLine("Unknown command!\nType 'help' for commands!");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine();
        }
    }
}



public class Commands {
    public static void Help() {
        Console.WriteLine("Commands: ");
        Console.WriteLine();
        Console.WriteLine("Clear            | Clears console");
        Console.WriteLine("ToggleDebug      | Sets Debug ON or OFF for console");
        Console.WriteLine("Users            | Users connected to server");
        Console.WriteLine("Connect          | Connect to server");
        Console.WriteLine("Disconnect       | Disconnect from server");
        Console.WriteLine("Status           | Check if connected to server");
        Console.WriteLine("SendData         | Sends a command to another client/server");
        Console.WriteLine("RequestData      | Gets a value from another client/server");
        Console.WriteLine("ClientMethods    | Methods available on client");
        Console.WriteLine("ServerMethods    | Methods available on server");
        Console.WriteLine("Exit             | Closes application");
    }
    public static void UserList() {
        if (Network.ClientList.Count() == 0) {
            Console.WriteLine("No other clients connected");
            return;
        }
        Console.WriteLine("Connected clients: ");

        int i = 1;
        foreach (Network.OtherClient _client in Network.ClientList) {
            Console.WriteLine($"    ({i}) ID={_client.ID} Name={_client.UserName}");
            i++;
        }
    }
    public static void SendData() {
        if (!Network.Client.HandshakeDone)
            throw new Exception("Connect to server first!");

        Console.WriteLine();
        Console.WriteLine("method to be sent to client/server: ");
        string? method = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("DATA string to be sent to client/server: ");
        string? data = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target;
        while (true) {
            target = Console.ReadLine();
            if (!string.IsNullOrEmpty(target)) break;
            Console.WriteLine("Invalid target, try again!");
        }

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = data == null ? null : new object[] {data},
            MethodName = method,
            TargetId = Int32.Parse(target)
        };
        Network.SendData(message);
    }
    public static void GetClientMethods() {
        Console.WriteLine();
        foreach (var item in Network.ClientMethods) Console.WriteLine($"{item.Name} ReturnType:({item.ReturnType})  ParamCount:({item.GetParameters().Count()})");
    }
    public static void GetServerMethods() {
        if (Network.ServerMethods == null) throw new Exception("Server Methods not Initialized yet! (Gets populated when when connected to server)");
        Console.WriteLine();
        foreach (var item in Network.ServerMethods) Console.WriteLine($"{item.Name} ReturnType:({item.ReturnType})  ParamCount:({(item.Parameters)?.Count()})");
    }
    public static void RequestData() {
        if (!Network.IsConnected())
            throw new Exception("Connect to server first!");

        Console.WriteLine();
        Console.WriteLine("Enter Method Name:");
        string? method = Console.ReadLine();

        Console.WriteLine();
        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target;
        while (true) {
            target = Console.ReadLine();
            if (!string.IsNullOrEmpty(target)) break;
            Console.WriteLine("Invalid target, try again!");
        }

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = "123",
            MethodName = method,
            TargetId = Int32.Parse(target)
        };

        dynamic a = Network.RequestData(message);
        
        if (a is Array)
            foreach (var b in a) {
                if (b is Array) {
                    foreach (var c in b) Console.WriteLine($"{c} ({c.GetType()})");
                    continue;
                }
                Console.WriteLine($"{b} ({b.GetType()})");
            }
        else 
            Console.WriteLine($"{a} ({a.GetType()})");
    }
    public static void RequestDataType() {
        if (!Network.IsConnected())
            throw new Exception("Connect to server first!");

        Console.WriteLine();
        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target;
        while (true) {
            target = Console.ReadLine();
            if (!string.IsNullOrEmpty(target)) break;
            Console.WriteLine("Invalid target, try again!");
        }

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = "Hello REQUEST From Client",
            MethodName = "getclassdata",
            TargetId = Int32.Parse(target)
        };
        TestClass? a = Network.RequestData<TestClass>(message);
        Console.WriteLine($"RETURNED:{a.Text}");
        Console.WriteLine($"RETURNED:{a.Data}");
    }
}