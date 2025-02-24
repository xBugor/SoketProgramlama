using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static List<TcpClient> clients = new List<TcpClient>();  // Bağlantılı istemciler
    private static List<string> kullaniciadlari = new List<string>(); // Kullanıcı adları
    private static Dictionary<string, TcpClient> clientDictionary = new Dictionary<string, TcpClient>(); // Kullanıcı adı ve istemci eşlemesi
    private static TcpListener serverListener;//serverin portunu dinlettiren 

    public static void Main()
    {
        string ipAddress = "10.141.0.32";
        int port = 6989;

        serverListener = new TcpListener(IPAddress.Parse(ipAddress), port);  // Veriyi IP adresine dönüştürme işlemi
        serverListener.Start();
        Console.WriteLine("STAJ MESAJLAŞMA UYGULAMAMIZ");
        Console.WriteLine($"Sunucu ip adresi {ipAddress}:{port}");

        while (true)
        {
            TcpClient clientSocket = serverListener.AcceptTcpClient();
            Console.WriteLine("Yeni bir istemci bağlandı.");


            //buraya lock koydum çünkü diğer clientleri bekletiyorum yoksa burda çakışma oluyor.
            lock (clients)
            {
                clients.Add(clientSocket);
            }
            //lambda fonksiyonları ile thread oluşturup çalıştırma
            Thread clientThread = new Thread(() => HandleClient(clientSocket));
            clientThread.Start();
        }
    }

    // İstemciyi işlemek için thread fonksiyonu
    private static void HandleClient(TcpClient clientSocket)
    {
        NetworkStream stream = clientSocket.GetStream();//tcp ile veri alıp verme
        byte[] buffer = new byte[1024];//gelen mesajları depolanması 
        int bytesRead;
        string username = "";

        try
        {
            // Bağlantı açıldığında kullanıcı adı al
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            string initialMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string[] parts = initialMessage.Split(':');
            username = parts[0]; // Kullanıcı adı ilk kısmı alır
            lock (clientDictionary)
            {
                clientDictionary[username] = clientSocket; // Kullanıcı adını ve soketi eşleştir
            }

            Console.WriteLine($"Yeni kullanıcı: {username}");

            // Kullanıcı adlarını listeliyoruz
            lock (kullaniciadlari)
            {
                kullaniciadlari.Add(username);
            }

            // Bağlantı açık olduğu sürece veri al
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Alınan mesaj: {message}");

                // Eğer mesaj '@' içeriyorsa, sadece hedef kullanıcıya ilet
                if (message.Contains('@'))
                {
                    string targetUser = message.Split('@')[1].Split(' ')[0]; // '@' karakterinden sonra gelen kısmı kullanıcı adı olarak al
                    if (clientDictionary.ContainsKey(targetUser)) // Eğer hedef kullanıcı listede varsa
                    {
                        BroadcastMessageToUser(message, targetUser);
                    }
                }
                else
                {
                    // Eğer '@' yoksa, tüm istemcilere mesaj gönder
                    BroadcastMessage(message, clientSocket);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
        finally
        {
            // Bağlantıyı kapat
            lock (clientDictionary)
            {
                clientDictionary.Remove(username); // Kullanıcı adı ve istemci eşlemesini sil
            }
            lock (clients)
            {
                clients.Remove(clientSocket); // İstemciyi listeden çıkar
            }
            clientSocket.Close();
        }
    }

    // Mesajı tüm bağlı istemcilere gönder
    private static void BroadcastMessage(string message, TcpClient sendingClient)
    {
        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);

        lock (clients)
        {
            foreach (TcpClient client in clients)
            {
                if (client != sendingClient)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(messageBuffer, 0, messageBuffer.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Mesaj gönderilemedi: " + ex.Message);
                    }
                }
            }
        }
    }

    // Belirli bir kullanıcıya mesaj gönder
    private static void BroadcastMessageToUser(string message, string targetUser)
    {
        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);

        lock (clientDictionary)
        {
            if (clientDictionary.ContainsKey(targetUser))
            {
                TcpClient targetClient = clientDictionary[targetUser];
                try
                {
                    NetworkStream stream = targetClient.GetStream();
                    stream.Write(messageBuffer, 0, messageBuffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Mesaj gönderilemedi: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine($"Kullanıcı {targetUser} bulunamadı.");
            }
        }
    }
}
