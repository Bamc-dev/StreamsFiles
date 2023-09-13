using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebSocket4Net;

public class WebSocketClient
{
    private WebSocket webSocket;
    private string receivedMessage;

    public event Action<string> MessageReceived;

    public WebSocketClient(string serverUri)
    {
        webSocket = new WebSocket(serverUri);
        webSocket.MessageReceived += WebSocket_MessageReceived;
    }

    public void Connect()
    {
        webSocket.Open();
    }

    public void Disconnect()
    {
        webSocket.Close();
    }

    private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        receivedMessage = "Message Received: " + e.Message;

        // Déclenchez l'événement MessageReceived
        MessageReceived?.Invoke(receivedMessage);
    }
}

    

