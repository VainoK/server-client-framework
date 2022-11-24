﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using static ServerFramework.Network;

//namespace ServerFramework;
public class TestClass {
    public bool Test { get; set; }
    public string? StringTest { get; set; }
    public dynamic? Data { get; set; }
}
public class ServerMethods {
    public static string Test(NetworkClient client, dynamic testMessage)
    {
        Console.WriteLine(
            $"MSG:{testMessage} ({testMessage.GetType()}) " +
            $"CLIENT: {client.Client.RemoteEndPoint} ID:{client.ID}"
        );
        return "Hello MSG RESPONSE From SERVER!";
    }
    public static int TestInt(NetworkClient client, dynamic testMessage)
    {
        Console.WriteLine($"MSG:{testMessage} CLIENT: {client.Client.RemoteEndPoint} ID:{client.ID}");
        return 123;
    }
    public static int TestTwo(NetworkClient client, string testMessage, int intt)
    {
        Console.WriteLine($"TestTwo:{testMessage} : {intt}");
        return 123;
    }
    public static int TestZero()
    {
        Console.WriteLine($"MSG: no params");
        return 123;
    }
    public static void TestVoid(NetworkClient client, dynamic testMessage)
    {
        Console.WriteLine($"This is a VOID method: {testMessage}");
    }
    public static dynamic TestType(NetworkClient client, dynamic testMessage)
    {
        Console.WriteLine($"MSG:{testMessage} CLIENT: {client.Client.RemoteEndPoint} ID:{client.ID}");
        TestClass test = new TestClass();
        test.StringTest = "TESTI";
        test.Test = true;
        test.Data = new string[] { "asd" };
        return test;
    }

    public static object[] TestArray(NetworkClient client, dynamic parameters)
    {
        return new object[] { "test", true, 1213 };
    }



    public static object[] ConnectedClients(NetworkClient client, dynamic parameters)
    {
        List<object[]> list = new List<object[]>();
        foreach (NetworkClient toAdd in ClientList)
        {
            if (!toAdd.Connected) continue;
            list.Add(new object[] { toAdd.ID, toAdd.UserName });
        }
        return list.ToArray();
    }
}