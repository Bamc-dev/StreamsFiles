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
    public delegate void MessageReceivedEventHandler(object sender, string message);
    public event MessageReceivedEventHandler MessagePlay;
    public event MessageReceivedEventHandler MessagePause;
    public event MessageReceivedEventHandler MessageUrl;
    public event MessageReceivedEventHandler MessageTime;


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

    public void SendMessage(string message)
    {
        webSocket.Send(message);
    }

    public async Task Disconnect()
    {
        webSocket.Close();
    }

    private void WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        receivedMessage = e.Message;
        if(receivedMessage.Contains("time:"))
            MessageTime?.Invoke(this,receivedMessage);
        else if (receivedMessage.Contains("url:"))
            MessageUrl?.Invoke(this, receivedMessage);
        else if (receivedMessage.Equals("play"))
            MessagePlay?.Invoke(this, receivedMessage);
        else if (receivedMessage.Equals("pause"))
            MessagePause?.Invoke(this, receivedMessage);
    }
    
}

    

