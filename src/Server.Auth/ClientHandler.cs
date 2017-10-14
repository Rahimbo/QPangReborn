using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Reborn.Utils;

namespace Server.Auth
{
    internal class ClientHandler
    {
        private readonly Socket _socket;
        private readonly IPEndPoint _remoteEndPoint;

        private readonly byte[] _buffer;

        public ClientHandler(Socket socket)
        {
            _socket = socket;
            _remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;
            _buffer = new byte[512];

            Console.WriteLine($"Accepted new connection from {_remoteEndPoint.Address}.");
        }

        public void WaitForData()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                var byteCount = _socket.EndReceive(ar);
                if (byteCount == 0)
                {
                    _socket.Close();
                    _socket.Dispose();

                    Console.WriteLine($"Connection from {_remoteEndPoint.Address} was dropped.");
                    return;
                }

                var packet = new byte[byteCount];
                Buffer.BlockCopy(_buffer, 0, packet, 0, byteCount);

                /* Packet processor */
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(BitConverter.ToString(packet));

                /* Packet 602 - LsLoginFail
                        packetList = new List<byte>(); // (?)
                        packetList.AddRange(BitEndianConverter.GetBytes((short)5, true));
                        packetList.AddRange(NetworkUtil.Pad(2));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)602, true)); // Packet ID
                        packetList.AddRange(NetworkUtil.Pad(4));
                        packetList.AddRange(BitEndianConverter.GetBytes(819, true)); // custom error
                        packetList.AddRange(Encoding.Unicode.GetBytes("Some kind of custom error")); // custom error
                        packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));
                        _socket.Send(packetList.ToArray());
                */

                /* test
                        packetList = new List<byte>(); // (?)
                        packetList.AddRange(BitEndianConverter.GetBytes((short)5, true));
                        packetList.AddRange(NetworkUtil.Pad(2));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)602, true)); // Packet ID
                        packetList.AddRange(Encoding.Default.GetBytes("This is a test."));
                        packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));
                        _socket.Send(packetList.ToArray());
                        Console.WriteLine(BitConverter.ToString(packetList.ToArray()));
                        Console.WriteLine("Bytes: " + packetList.Count);
                */

                var packetLength = EncodeHelper.DecodeShort(new[] { packet[1], packet[0] });
                var packetHeader = packet[2]; // Not sure :)

                Console.WriteLine($"Packet Length [Short]: {packetLength}");
                Console.WriteLine($"Packet ID [Byte]: {packetHeader}");
                Console.WriteLine($"Packet Raw: {Encoding.UTF8.GetString(packet)}");
                Console.WriteLine($"Packet Raw Unicode: {Encoding.Unicode.GetString(packet)}");
                Console.WriteLine($"Received bytes: {byteCount}");

                var packetList = new List<byte>();
                switch (packetHeader)
                {
                    case 255: // Connect to port 8005??
                        packetList.AddRange(BitEndianConverter.GetBytes((short)5, true));
                        packetList.AddRange(EncodeHelper.CreatePadding(2));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)501, true)); // Packet ID
                        packetList.AddRange(EncodeHelper.CreatePadding(4));
                        packetList.Add(0x7F); // 127
                        packetList.Add(0x0);  // 0
                        packetList.Add(0x0);  // 0
                        packetList.Add(0x1);  // 1

                        break;
                    case 5:
                        /* packetList.AddRange(BitEndianConverter.GetBytes((short)5, true));
                        packetList.AddRange(NetworkUtil.Pad(2));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)502, true)); // Packet ID
                        packetList.AddRange(NetworkUtil.Pad(4));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)1101, true)); */

                        Console.ForegroundColor = ConsoleColor.Green;

                        packetList = new List<byte>(); // Show server list
                        packetList.AddRange(BitEndianConverter.GetBytes((short)5, true));
                        packetList.AddRange(EncodeHelper.CreatePadding(2));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)601, true)); // Packet ID
                        packetList.AddRange(EncodeHelper.CreatePadding(4));
                        packetList.AddRange(Encoding.ASCII.GetBytes("Cool"));
                        packetList.AddRange(EncodeHelper.CreatePadding(42));
                        packetList.AddRange(Encoding.Unicode.GetBytes("Test"));
                        packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));
                        _socket.Send(packetList.ToArray());
                        Console.WriteLine(BitConverter.ToString(packetList.ToArray()));
                        Console.WriteLine("Bytes: " + packetList.Count);

                        packetList = new List<byte>();

                        break;

                    case 3:
                        packetList.AddRange(BitEndianConverter.GetBytes((short)3, true));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)1, true));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)2, true));

                        break;
                        
                    default:
                        // Console.ForegroundColor = ConsoleColor.Red;
                        // Console.WriteLine("Unknown packet, trying to respond with bullshit packet.");

                        /* packetList.AddRange(BitEndianConverter.GetBytes((short)0, false));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)30, true));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)0, false));
                        packetList.AddRange(BitEndianConverter.GetBytes((long)0, false)); */
                        break;
                }

                if (packetList.Count > 0)
                {
                    packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(BitConverter.ToString(packetList.ToArray()));
                    Console.WriteLine("Bytes: " + packetList.Count);

                    _socket.Send(packetList.ToArray());
                }

                /* Finalize */
                Console.ResetColor();
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception);
                Console.ResetColor();
            }
            finally
            {
                WaitForData();
            }
        }
    }
}