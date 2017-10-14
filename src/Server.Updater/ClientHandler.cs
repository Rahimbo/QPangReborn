using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Reborn.Utils;

namespace Server.Updater
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
                Console.WriteLine(BitConverter.ToString(packet).Replace("-", ""));

                var packetLength = EncodeHelper.DecodeShort(new[] { packet[1], packet[0] });
                var packetHeader = EncodeHelper.DecodeShort(new[] { packet[3], packet[2] });
                var command = EncodeHelper.DecodeShort(new[] { packet[7], packet[6] });

                Console.WriteLine($"Packet Length [Short]: {packetLength}");
                Console.WriteLine($"Packet ID [Short]: {packetHeader}");
                Console.WriteLine($"Command [Short]: {command}");
                Console.WriteLine($"Packet Raw: {Encoding.UTF8.GetString(packet)}");

                /* 0  1  2  3  4  5  6  7  8  9  10 11 12 13 14 15
                 * 10-00-11-00-04-00-01-00-01-00-00-00-E4-00-00-00
                 * | Length
                 *       | ID
                 *             |?
                 *                   | CMD
                 *
                 * Second Packet:
                 *   CMD 7&8: Launcht het spel / gameguard update
                 */

                /* Packet responder */
                var packetList = new List<byte>();
                switch (command)
                {
                    case 1:
                        /* Error example 
                        List<byte> packetList = new List<byte>();
                        packetList.AddRange(Encoder.CreatePadding(6)); // pos 0,1,2,3,4,5 (6)
                        packetList.AddRange(Encoder.EncodeShort(99).Reverse()); // COMMAND pos 6,7 (2)
                        packetList.AddRange(Encoder.CreatePadding(4)); // pos 8,9,10,11 (4)
                        packetList.AddRange(Encoder.EncodeInteger(1337).Reverse()); // pos 12,13,14,15 (5)
                        packetList.AddRange(Encoding.Unicode.GetBytes("l33t")); // pos 16+ */

                        packetList.AddRange(EncodeHelper.CreatePadding(6));
                        packetList.AddRange(EncodeHelper.EncodeShort(2).Reverse());
                        packetList.AddRange(EncodeHelper.CreatePadding(8));

                        _socket.Send(packetList.ToArray());
                        break;
                    case 6:
                        packetList.AddRange(EncodeHelper.CreatePadding(6));
                        packetList.AddRange(EncodeHelper.EncodeShort(8).Reverse());
                        packetList.AddRange(EncodeHelper.CreatePadding(4));

                        _socket.Send(packetList.ToArray());
                        break;
                }

                /* Finalize */
                Console.ResetColor();
                Console.WriteLine("Received bytes: " + byteCount);
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception.StackTrace);
                Console.ResetColor();
            }
            finally
            {
                WaitForData();
            }
        }
    }
}