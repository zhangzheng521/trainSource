using System;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient.Newtonsoft.Json;
using SocketIOClient;
using System.Net.Sockets;
using UnityEngine;

public class NetSocket : MonoBehaviour
{
    public string ip="127.0.0.1";
    public string port = "8080";

    public SocketIOUnity socket;

    private void Awake()
    {
        var uri = new Uri("http://"+ip+":"+port);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            EIO = 4
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("The socket connection is successful.");
        };
        socket.OnPing += (sender, e) =>
        {
            Debug.Log("Ping");
        };
        socket.OnPong += (sender, e) =>
        {
            Debug.Log("Pong: " + e.TotalMilliseconds);
        };
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        socket.OnReconnectError += (sender, e) =>
        {
            Debug.Log("ReconnectError:" + e.Message);
        };

        Debug.Log("Connecting...");
        socket.Connect();
    }

    private void Socket_OnError(object sender, string e)
    {
        throw new NotImplementedException();
    }
}
