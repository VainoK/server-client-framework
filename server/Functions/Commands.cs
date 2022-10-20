﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerFramework {
    public class Commands {
        public static void Help() {
            Console.WriteLine("Commands: ");
            Console.WriteLine();
            Console.WriteLine("Clear        | Clears console");
            Console.WriteLine("Users        | Users connected to server");
            Console.WriteLine("Start        | Start server");
            Console.WriteLine("Stop         | Stop server");
            Console.WriteLine("Status       | Check if server is running");
            Console.WriteLine("SendData     | Sends a command to user(s)");
            Console.WriteLine("RequestData  | Requests data from user");
            Console.WriteLine("Exit         | Closes server");
        }
        public static void UserList() {
            if (Network.ClientList.Count() == 0)
                throw new Exception("No users connected!");

            Console.WriteLine("Connected clients count: " + Network.ClientList.Count());
            foreach (NetworkClient client in Network.ClientList) {
                string remoteIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                Console.WriteLine("    User: " + client.UserName + " - (" + remoteIP + ") : ID=" + client.ID.ToString());
            }
        }
        public static void SendCommand() {
            if (!Network.ServerRunning)
                throw new Exception("Start the server first!");

            if (Network.ClientList.Count() == 0)
                throw new Exception("No clients online!");

            Console.WriteLine();
            Console.WriteLine("method to be sent to client: ");
            string method = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine("Target ID: (Blank or 0 for all clients)");
            string target = Console.ReadLine();
            if (string.IsNullOrEmpty(target))
                target = "0";

            Network.NetworkMessage message = new Network.NetworkMessage {
                Parameters = Network.SerializeParameters("TEST MSG FROM SERVER"),
                TargetId = Int32.Parse(target),
                MethodId = Network.GetMethodIndex(method)
            };
            Network.SendData(message);
        }
        public static void RequestData() {
            if (!Network.ServerRunning)
                throw new Exception("Start the server first!");

            if (Network.ClientList.Count() == 0)
                throw new Exception("No clients online!");

            Console.WriteLine();
            Console.WriteLine("method to be sent to client: ");
            string method = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine("Target ID: (Blank or 0 for all clients)");
            string target = Console.ReadLine();
            if (string.IsNullOrEmpty(target))
                target = "0";

            Network.NetworkMessage message = new Network.NetworkMessage {
                Parameters = Network.SerializeParameters("TEST MSG FROM SERVER"),
                TargetId = Int32.Parse(target),
                MethodId = Network.GetMethodIndex(method)
            };
            object[] data = Network.RequestData(message);
            Console.WriteLine(data[0]);
        }
    }
}
