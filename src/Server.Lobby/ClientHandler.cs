using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Reborn.Utils;

namespace Server.Lobby
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
                Console.WriteLine("Waiting for new data");
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int byteCount = _socket.EndReceive(ar);

                if (byteCount == 0)
                {
                    _socket.Close();
                    _socket.Dispose();

                    Console.WriteLine($"Lost connection from {_remoteEndPoint.Address}.");
                    return;
                }

                var packet = new byte[byteCount];
                Buffer.BlockCopy(_buffer, 0, packet, 0, byteCount);

                /* Packet processor */
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(BitConverter.ToString(packet));

                var packetLength = EncodeHelper.DecodeShort(new[] { packet[1], packet[0] });
                var packetHeader = packet[2]; // Not sure :)

                Console.WriteLine($"Packet Length [Short]: {packetLength}");
                Console.WriteLine($"Packet ID [Byte]: {packetHeader}");
                Console.WriteLine($"Packet Raw: {Encoding.UTF8.GetString(packet)}");
                Console.WriteLine($"Packet Raw Unicode: {Encoding.Unicode.GetString(packet)}");

                var packetList = new List<byte>();
                switch (packetHeader)
                {
                    case 5:
                        packetList.AddRange(EncodeHelper.CreatePadding(4));
                        packetList.AddRange(BitEndianConverter.GetBytes((short)502, true)); // Packet ID
                        packetList.AddRange(EncodeHelper.CreatePadding(4));
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);
                        packetList.Add(0x05);

                        packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(BitConverter.ToString(packetList.ToArray()));
                        Console.WriteLine("Bytes: " + packetList.Count);

                        _socket.Send(packetList.ToArray());

                        break;

                    case 3:
                        packetList.Add(0x03);
                        packetList.Add(0x04);
                        packetList.Add(0x01);
                        packetList.Add(0x02);
                        packetList.Add(0x02); // Must be 2?

                        packetList.InsertRange(0, BitEndianConverter.GetBytes((short)(packetList.Count + 2), true));

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(BitConverter.ToString(packetList.ToArray()));
                        Console.WriteLine("Bytes: " + packetList.Count);

                        _socket.Send(packetList.ToArray());

                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown packet received, trying to send it back lol.");

                        _socket.Send(packet);

                        break;
                }

                /* Finalize */
                Console.ResetColor();
                Console.WriteLine("Received bytes: " + byteCount);
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