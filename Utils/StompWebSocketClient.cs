using System;
using System.Diagnostics;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Net;

public class StompWebSocketClient
{
    private WebSocket webSocket;

    public event Action<string> OnMessageReceived;

    public StompWebSocketClient(string serverUrl)
    {
        webSocket = new WebSocket(serverUrl);

        webSocket.OnMessage += (sender, e) =>
        {
            if (e.IsText)
            {
                var stompMessage = e.Data;
                Debug.WriteLine("Message STOMP reçu : " + stompMessage);

                // Traitez le message STOMP ici
                OnMessageReceived?.Invoke(stompMessage);
            }
        };

        webSocket.OnError += (sender, e) =>
        {
            Debug.WriteLine("Erreur WebSocket : " + e.Message);
        };

        webSocket.OnClose += (sender, e) =>
        {
            Debug.WriteLine("Connexion WebSocket fermée.");
        };
    }

    public void Connect()
    {
        webSocket.Connect();
    }

    public void Disconnect()
    {
        webSocket.Close();
    }

    public void SendStompMessage(string stompMessage)
    {
        webSocket.Send(stompMessage);
    }

    public void Subscribe(string destination)
    {
        // Créez un message STOMP d'abonnement au canal spécifié
        var subscribeMessage = $"SUBSCRIBE\nid:sub-1\ndestination:{destination}\n\n";
        SendStompMessage(subscribeMessage);
    }
}
