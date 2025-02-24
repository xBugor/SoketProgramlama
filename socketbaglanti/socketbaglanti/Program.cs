using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private static TcpClient clientSocket;
    private static NetworkStream stream;

    public static void Main()
    {
        string ipAddress = "10.141.0.32";
        int port = 6989;

        // Sunucuya bağlan
        clientSocket = new TcpClient(ipAddress, port);
        stream = clientSocket.GetStream();

        Console.WriteLine("Sunucuya bağlandı.");

        // Kullanıcı adı al
        Console.Write("Kullanıcı adı girin: ");
        string username = Console.ReadLine();

        // Kullanıcı adı ile sunucuya giriş yap
        SendMessage(username + ":Giriş yaptı.");

        // Kullanıcı ile mesajlaşma başlat
        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        while (true)
        {
            string message = Console.ReadLine();

            // Kullanıcı '@' kullanarak mesaj gönderebilir
            if (message.Contains('@'))
            {
                SendMessage(message);
            }
            else
            {
                // Diğer mesajlar tüm kullanıcılara gider
                SendMessage(username + ":" + message);
            }
        }
    }

    // Sunucuya mesaj gönderme
    private static void SendMessage(string message)
    {
        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
        stream.Write(messageBuffer, 0, messageBuffer.Length);
    }

    // Sunucudan mesajları alma
    private static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
    }
}