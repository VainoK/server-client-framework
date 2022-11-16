﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ServerFramework;

public class Test {
    public static void Aasd() {
        
    }
}
public class Program {

    public static void OnClientConnected(object sender, OnClientConnectEvent eventData){
        Console.WriteLine($"*EVENT* CLIENT CONNECTED! ({eventData.UserName} ID:{eventData.Id} SUCCESS:{eventData.Success})");
    }
    public static void OnClientDisconnect(object sender, OnClientDisconnectEvent eventData){
        Console.WriteLine($"*EVENT* CLIENT DISCONNECTED! ({eventData.UserName} ID:{eventData.Id} SUCCESS:{eventData.Success})");
    }
    public static void OnServerShutdown(object sender, OnServerShutdownEvent eventData){
        Console.WriteLine($"*EVENT* SERVER STOPPED! SUCCESS:{eventData.Success}");
    }
    public static void OnMessageSent(object sender, OnMessageSentEvent eventData){
        Console.WriteLine($"*EVENT* MSG SENT: {eventData.Message.MethodName}");
    }
    public static void OnMessageReceived(object sender, OnMessageReceivedEvent eventData){
        Console.WriteLine($"*EVENT* MSG RECEIVED: {eventData.Message.MethodName}");
    }



    
    static void Main(string[] args) {
        Console.Clear();
        Console.Title = "SERVER";
        Console.WriteLine("Type 'help' for commands!");

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        Console.WriteLine(version);

        int methodsAdded = Network.RegisterMethod( typeof(ServerMethods) );
        Console.WriteLine($"{methodsAdded} Methods registered!");

        NetworkEvents.eventsListener = new NetworkEvents();
        NetworkEvents.eventsListener.ClientConnected += OnClientConnected;
        NetworkEvents.eventsListener.ClientDisconnect += OnClientDisconnect;
        NetworkEvents.eventsListener.ServerShutdown += OnServerShutdown;
        NetworkEvents.eventsListener.MessageSent += OnMessageSent;
        NetworkEvents.eventsListener.MessageReceived += OnMessageReceived;

        Console.WriteLine("Type 'help' for commands!");
    
        Network.StartServer(5001);

        while (true) {
            Console.WriteLine();
            string command = Console.ReadLine();
            command = command.ToLower();
        
            try {
                switch (command) {
                    case "help":
                        Commands.Help();
                        break;
                    
                    case "toggledebug":
                        Logger.Debug = !Logger.Debug;
                        break;

                    case "clear":
                        Console.Clear();
                        break;

                    case "exit":
                        break;

                    case "start":
                        if (Network.ServerRunning) throw new Exception("Server already running!");
                        Console.WriteLine("Enter server port:");
                        string portNew = Console.ReadLine();
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