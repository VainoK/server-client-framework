using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

using static ServerFramework.NetworkEvents;

namespace ServerFramework;

public class Test {
    public static void Aasd() {
        
    }
}
public class Program {

    public static void OnClientConnected(object sender, OnClientConnectEvent eventData){
        Console.WriteLine($"*EVENT* CLIENT CONNECTED! ({eventData.UserName} ID:{eventData.ClientID} SUCCESS:{eventData.Success})");
    }
    public static void OnClientDisconnect(object sender, OnClientDisconnectEvent eventData){
        Console.WriteLine($"*EVENT* CLIENT DISCONNECTED! ({eventData.UserName} ID:{eventData.ClientID} SUCCESS:{eventData.Success})");
    }
    public static void OnServerStart(object sender, OnServerStartEvent eventData){
        Console.WriteLine($"*EVENT* SERVER STARTED! SUCCESS:{eventData.Success}");
    }
    public static void OnServerShutdown(object sender, OnServerShutdownEvent eventData){
        Console.WriteLine($"*EVENT* SERVER STOPPED! SUCCESS:{eventData.Success}");
    }
    public static void OnMessageSent(object sender, OnMessageSentEvent eventData){
        Console.WriteLine($"*EVENT* MSG SENT: {eventData.Message?.MethodName}");
    }
    public static void OnMessageReceived(object sender, OnMessageReceivedEvent eventData){
        Console.WriteLine($"*EVENT* MSG RECEIVED: {eventData.Message?.MethodName}");
    }
    public static void OnHandShakeStart(object sender, OnHandShakeStartEvent eventData){
        Console.WriteLine($"*EVENT* HANDSHAKE STARTED: version:{eventData.ClientVersion}, username:{eventData.UserName}");
    }
    public static void OnHandShakeEnd(object sender, OnHandShakeEndEvent eventData){
        Console.WriteLine($"*EVENT* HANDSHAKE ENDED: version:{eventData.ClientVersion}, username:{eventData.UserName}");
    }



    
    static void Main(string[] args) {
        Console.Clear();
        Console.Title = "SERVER";
        Console.WriteLine("Type 'help' for commands!");

        Logger.Debug = true;
        //Settings.AllowSameUsername = false;

        int methodsAdded = Network.RegisterMethod( typeof(ServerMethods) );
        Console.WriteLine($"{methodsAdded} Methods registered!");

        NetworkEvents.Listener.ClientConnected += OnClientConnected;
        NetworkEvents.Listener.ClientDisconnect += OnClientDisconnect;
        NetworkEvents.Listener.ServerShutdown += OnServerShutdown;
        NetworkEvents.Listener.ServerStart += OnServerStart;
        NetworkEvents.Listener.MessageSent += OnMessageSent;
        NetworkEvents.Listener.MessageReceived += OnMessageReceived;
        NetworkEvents.Listener.HandshakeStart += OnHandShakeStart;
        NetworkEvents.Listener.HandshakeEnd += OnHandShakeEnd;
    
        Network.StartServer();

        while (true) {
            Console.WriteLine();
            string? command = Console.ReadLine();
            command = command?.ToLower();
        
            try {
                switch (command) {
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
                        break;

                    case "start":
                        if (Network.ServerRunning) throw new Exception("Server already running!");
                        Console.WriteLine("Enter server port:");
                        string? portNew = Console.ReadLine();
                        if (String.IsNullOrEmpty(portNew)) portNew = "5001";
                        Network.StartServer(Int32.Parse(portNew));
                        break;

                    case "stop":
                        Network.StopServer();
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
                    
                    case "sendevent":
                        Commands.SendEvent();
                        break;
                    
                    case "status":
                        Console.WriteLine(Network.ServerRunning ? "Server is running!" : "Server is not running!");
                        break;

                    default:
                        Console.WriteLine("Unknown command!" + "\n" + "Type 'help' for commands!");
                        break;
                }
            } catch (Exception e) {
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
        Console.WriteLine("Clear        | Clears console");
        Console.WriteLine("ToggleDebug  | Sets Debug ON or OFF for console");
        Console.WriteLine("Users        | Users connected to server");
        Console.WriteLine("Start        | Start server");
        Console.WriteLine("Stop         | Stop server");
        Console.WriteLine("Status       | Check if server is running");
        Console.WriteLine("SendData     | Sends a command to user(s)");
        Console.WriteLine("RequestData  | Requests data from user");
        Console.WriteLine("ClientMethods| Methods available on client");
        Console.WriteLine("ServerMethods| Methods available on server");
        Console.WriteLine("Exit         | Closes server");
    }
    public static void UserList() {
        if (Network.ClientList.Count() == 0)
            throw new Exception("No users connected!");

        Console.WriteLine("Connected clients count: " + Network.ClientList.Count());
        foreach (Network.NetworkClient? client in Network.ClientList) {
            EndPoint? endPoint = client.Client.RemoteEndPoint;
            string? remoteIP = (endPoint as IPEndPoint)?.Address?.ToString();
            Console.WriteLine("    User: " + client.UserName + " - (" + remoteIP + ") : ID=" + client.ID.ToString());
        }
    }
    public static void SendData() {
        if (!Network.ServerRunning)
            throw new Exception("Start the server first!");

        if (Network.ClientList.Count() == 0)
            throw new Exception("No clients online!");

        Console.WriteLine();
        Console.WriteLine("method to be sent to client: ");
        string? method = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target = Console.ReadLine();
        if (string.IsNullOrEmpty(target))
            target = "0";

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = "TEST MSG FROM SERVER",
            TargetId = Int32.Parse(target),
            MethodName = method
        };
        Network.SendData(message);
    }
    public static void GetClientMethods() {
        if (Network.ClientMethods == null) throw new Exception("Client Methods not Initialized yet! (Gets populated when first client joins)");
        Console.WriteLine();
        foreach (var item in Network.ClientMethods) Console.WriteLine($"{item.Name} ReturnType:({item.ReturnType})  ParamCount:({(item.Parameters)?.Count()})");
    }
    public static void GetServerMethods() {
        Console.WriteLine();
        foreach (var item in Network.ServerMethods) Console.WriteLine($"{item.Name} ReturnType:({item.ReturnType})  ParamCount:({item.GetParameters().Count()})");   
    }
    public static void SendEvent() {
        Network.NetworkEvent eventData = new Network.NetworkEvent(new OnServerShutdownEvent(true));
        Network.SendEvent(eventData);
    }
    public static void RequestData() {
        if (!Network.ServerRunning)
            throw new Exception("Start the server first!");

        if (Network.ClientList.Count() == 0)
            throw new Exception("No clients online!");

        Console.WriteLine();
        Console.WriteLine("method to be sent to client: ");
        string? method = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target = Console.ReadLine();
        if (string.IsNullOrEmpty(target))
            target = "0";

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = new object[] {"TEST MSG FROM SERVER"},
            TargetId = Int32.Parse(target),
            MethodName = method
        };
        dynamic a = Network.RequestData(message);
        if (a is Array)
            foreach (var b in a) Console.WriteLine($"{b} ({b.GetType()})");
        else 
            Console.WriteLine($"{a} ({a.GetType()})");
    }
    public static void RequestDataType() {
        if (!Network.ServerRunning)
            throw new Exception("Start the server first!");

        if (Network.ClientList.Count() == 0)
            throw new Exception("No clients online!");

        Console.WriteLine("Target ID: (Blank or 0 for all clients)");
        string? target = Console.ReadLine();
        if (string.IsNullOrEmpty(target))
            target = "0";

        Network.NetworkMessage message = new Network.NetworkMessage {
            Parameters = "Hello From Client",
            MethodName = "TestType",
            TargetId = Int32.Parse(target)
        };
        TestClass a = Network.RequestData<TestClass>(message);
        Console.WriteLine($"RETURNED:{a}");
        Console.WriteLine($"RETURNED:{a.Data?[0]}");
    }
}