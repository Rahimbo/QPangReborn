using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Auth
{
    /// <summary>
    ///     Prototype AuthServer.
    ///     
    ///     QPang target: 2012-05-02 (s2)
    ///     IP: qpanggame.realfogs.nl
    ///     Port: 8003
    /// 
    ///     Modify the ip to 127.0.0.1 by opening C:\Windows\System32\drivers\etc\hosts
    ///     and adding this line: "qpanggame.realfogs.nl 127.0.0.1".
    /// </summary>
    internal class Program
    {
        private static bool _keepRunning;

        private static Socket _serverSocket;

        public static void Main(string[] args)
        {
            Console.Title = "QPangReborn | Server.Auth PROTOTYPE";
            Console.WriteLine("Starting up Server.Auth.");

            _keepRunning = true;
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 8003));
            _serverSocket.Listen(20);

            AcceptNewConnection();

            while (_keepRunning)
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "exit":
                    case "quit":
                    case "q":
                        _keepRunning = false;
                        break;
                }
            }

            Console.WriteLine("Shutting down Server.Auth.");

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
            if (_serverSocket == null)
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
