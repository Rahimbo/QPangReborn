using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Lobby
{
    /// <summary>
    ///     Prototype LobbyServer.
    ///     
    ///     QPang target: 2012-05-02 (s2)
    ///     IP: Depends on "case 255" in Server.Auth.ClientHandler
    ///     Port: 8005
    /// </summary>
    internal class Program
    {
        private static bool _keepRunning = true;

        private static Socket _serverSocket;

        private static void Main()
        {
            Console.Title = "QPangReborn | Server.Lobby PROTOTYPE";
            Console.WriteLine("Starting up Server.Lobby.");

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8005));
            _serverSocket.Listen(20);

            AcceptNewConnection();

            while (_keepRunning)
            {
                string command = Console.ReadLine();
                switch (command)
                {
                    case "exit":
                    case "quit":
                    case "q":
                        _keepRunning = false;
                        break;
                }
            }

            Console.WriteLine("Shutting down MangaFighter lobby server..");

            _serverSocket.Close();
            _serverSocket.Dispose();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void AcceptNewConnection()
        {
            Console.WriteLine("Waiting for a new connection.");

            _serverSocket?.BeginAccept(AcceptConnection, null);
        }

        private static void AcceptConnection(IAsyncResult ar)
        {
            if (_serverSocket == null || !_keepRunning)
            {
                return;
            }

            var clientSocket = _serverSocket.EndAccept(ar);

            new Thread(() => new ClientHandler(clientSocket).WaitForData())
            {
                IsBackground = true
            }.Start();

            AcceptNewConnection();
        }
    }
}
