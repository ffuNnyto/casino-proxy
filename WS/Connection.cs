using System.Net;
using System.Net.Sockets;
using System.Text;
using Games.Hacksaw.DataBase;
using Games.Pragmatic.DataBase;
using NetCoreServer;
using Newtonsoft.Json;

namespace WS.Connection;

class ClientSession : WsSession
{
    public ClientSession(WsServer server) : base(server) { }
    public override void OnWsConnected(HttpRequest request)
    {
        string CreateID = this.Id.ToString();
    }
    public override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        try
        {
            string Message = Encoding.Default.GetString(buffer, (int)offset, (int)size);

            if (Message == "REQUEST_DATA")
            {
                this.SendAsync(ConnectionUtils.CreateFrameFromString(JsonConvert.SerializeObject(new {
                    hacksawdb = HacksawDataBase.DB,
                    pragmaticdb = PragmaticDataBase.DB
                }), ConnectionUtils.Opcode.Text));
            }
        }
        catch
        {

        }
    }
}

class ServerConnection : WsServer
{
    public ServerConnection(IPAddress address, int port) : base(address, port) { }
    protected override TcpSession CreateSession() { return new ClientSession(this); }
}
class ConnectionUtils
{
    public static byte[] CreateFrameFromString(string message, Opcode opcode = Opcode.Text)
    {
        var payload = Encoding.Default.GetBytes(message);
        byte[] frame;
        if (payload.Length < 126)
        {
            frame = new byte[1 + 1 + payload.Length];
            frame[1] = (byte)payload.Length;
            Array.Copy(payload, 0, frame, 2, payload.Length);
        }
        else if (payload.Length >= 126 && payload.Length <= 65535)
        {
            frame = new byte[1 + 1 + 2 + payload.Length];
            frame[1] = 126;
            frame[2] = (byte)((payload.Length >> 8) & 255);
            frame[3] = (byte)(payload.Length & 255);
            Array.Copy(payload, 0, frame, 4, payload.Length);
        }
        else
        {
            frame = new byte[1 + 1 + 8 + payload.Length];
            frame[1] = 127;
            frame[2] = (byte)((payload.Length >> 56) & 255);
            frame[3] = (byte)((payload.Length >> 48) & 255);
            frame[4] = (byte)((payload.Length >> 40) & 255);
            frame[5] = (byte)((payload.Length >> 32) & 255);
            frame[6] = (byte)((payload.Length >> 24) & 255);
            frame[7] = (byte)((payload.Length >> 16) & 255);
            frame[8] = (byte)((payload.Length >> 8) & 255);
            frame[9] = (byte)(payload.Length & 255);
            Array.Copy(payload, 0, frame, 10, payload.Length);
        }
        frame[0] = (byte)((byte)opcode | 0x80);
        return frame;
    }
    public static byte[] ParsePayloadFromFrame(byte[] incomingFrameBytes)
    {
        var payloadLength = 0L;
        var totalLength = 0L;
        var keyStartIndex = 0L;
        if ((incomingFrameBytes[1] & 0x7F) < 126)
        {
            payloadLength = incomingFrameBytes[1] & 0x7F;
            keyStartIndex = 2;
            totalLength = payloadLength + 6;
        }
        if ((incomingFrameBytes[1] & 0x7F) == 126)
        {
            payloadLength = BitConverter.ToInt16(new[] { incomingFrameBytes[3], incomingFrameBytes[2] }, 0);
            keyStartIndex = 4;
            totalLength = payloadLength + 8;
        }
        if ((incomingFrameBytes[1] & 0x7F) == 127)
        {
            payloadLength = BitConverter.ToInt64(new[] { incomingFrameBytes[9], incomingFrameBytes[8], incomingFrameBytes[7], incomingFrameBytes[6], incomingFrameBytes[5], incomingFrameBytes[4], incomingFrameBytes[3], incomingFrameBytes[2] }, 0);
            keyStartIndex = 10;
            totalLength = payloadLength + 14;
        }
        if (totalLength > incomingFrameBytes.Length)
        {
            throw new Exception("The buffer length is smaller than the data length.");
        }
        var payloadStartIndex = keyStartIndex + 4;
        byte[] key = { incomingFrameBytes[keyStartIndex], incomingFrameBytes[keyStartIndex + 1], incomingFrameBytes[keyStartIndex + 2], incomingFrameBytes[keyStartIndex + 3] };
        var payload = new byte[payloadLength];
        Array.Copy(incomingFrameBytes, payloadStartIndex, payload, 0, payloadLength);
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(payload[i] ^ key[i % 4]);
        }
        return payload;
    }
    public enum Opcode
    {
        Fragment = 0,
        Text = 1,
        Binary = 2,
        CloseConnection = 8,
        Ping = 9,
        Pong = 10
    }
}