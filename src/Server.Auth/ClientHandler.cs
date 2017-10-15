using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NLog;
using Reborn.Utils;
using Server.Auth.Net;

namespace Server.Auth
{
    internal class ClientHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Socket _socket;

        private readonly int _clientId;

        private readonly IPEndPoint _remoteEndPoint;

        private readonly byte[] _buffer;

        public ClientHandler(Socket socket, int clientId)
        {
            _socket = socket;
            _clientId = clientId;
            _remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;
            _buffer = new byte[512];

            Logger.Debug($"[{_clientId}] Accepted new connection from {_remoteEndPoint.Address}.");
        }

        public void WaitForData()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
        }
        
        public void SendPacket(byte[] rawPacket)
        {
            Logger.Warn($"[{_clientId}] Outgoing packet:{Environment.NewLine}" +
                         $"Packet Length: {rawPacket.Length}{Environment.NewLine}" +
                         $"Packet ID: {EncodeHelper.DecodeShort(new []{rawPacket[6], rawPacket[7]}, true)}{Environment.NewLine}" +
                         $"Packet Hex dump:{Environment.NewLine}" +
                         $"{HexUtils.HexDump(rawPacket)}");

            _socket.Send(rawPacket);
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

                    Logger.Debug($"[{_clientId}] Connection from {_remoteEndPoint.Address} was dropped.");
                    return;
                }

                var packet = new byte[byteCount];
                Buffer.BlockCopy(_buffer, 0, packet, 0, byteCount);

                /*  Packet 502 - RsLoginFail

                    new Packet(502)
                        .Append(EncodeHelper.CreatePadding(4))
                        .Append(EncodeHelper.EncodeShort(1101, true));
                */

                /*  Packet 602 - LsLoginFail

                    new Packet(602)
                        .Append(EncodeHelper.CreatePadding(4))
                        .Append(EncodeHelper.EncodeInteger(819, true))                      // Custom error
                        .Append(Encoding.Unicode.GetBytes("Some kind of custom error"));    // Custom error
                */

                var packetLength = EncodeHelper.DecodeShort(new[] { packet[1], packet[0] });
                var packetHeader = packet[2]; // Not sure :)

                Logger.Trace($"[{_clientId}] Incoming packet:{Environment.NewLine}" +
                             $"Packet Length Raw: {packet.Length}{Environment.NewLine}" +
                             $"Packet Length: {packetLength}{Environment.NewLine}" +
                             $"Packet ID: {packetHeader}{Environment.NewLine}" +
                             $"Packet Hex:{Environment.NewLine}" +
                             $"{HexUtils.HexDump(packet)}");

                var packetList = new List<byte[]>();
                switch (packetHeader)
                {
                    case 255:
                    {
                        // Connect to port 8005??
                        packetList.Add(new Packet(501)
                            .Append(EncodeHelper.CreatePadding(4))
                            .Append(0x74)
                            .Append(0x0)
                            .Append(0x0)
                            .Append(0x1)
                            .GetPacket());

                        break;
                    }

                    case 5:
                    {
                        packetList.Add(new Packet(601)
                            .Append(EncodeHelper.CreatePadding(4))
                            .Append(Encoding.ASCII.GetBytes("Cool"))
                            .Append(EncodeHelper.CreatePadding(42))
                            .Append(Encoding.Unicode.GetBytes("Test"))
                            .GetPacket());
                        break;
                    }

                    case 3:
                    {
                        var tempPacket = new List<byte>();

                        tempPacket.AddRange(EncodeHelper.EncodeShort(3, true));
                        tempPacket.AddRange(EncodeHelper.EncodeShort(1, true));
                        tempPacket.AddRange(EncodeHelper.EncodeShort(2, true));
                        tempPacket.InsertRange(0, EncodeHelper.EncodeShort((short) (tempPacket.Count + 2), true));

                        packetList.Add(tempPacket.ToArray());
                        break;
                    }
                        
                    default:
                        Logger.Error($"[{_clientId}] Incoming packet was not known.");
                        break;
                }

                if (packetList.Count > 0)
                {
                    foreach (var outgoingPacket in packetList)
                    {
                        SendPacket(outgoingPacket);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"[{_clientId}] An exception occured during receive.");
            }
            finally
            {
                WaitForData();
            }
        }
    }
}