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

    public WebSocketClient(string serverUri)
    {
        webSocket = new WebSocket(serverUri);

        // Gérez la réception des messages
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
        // Affichez le message dans un MessageBox
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show("Message Received: " + e.Message, "WebSocket Message", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }
}