using Kernys.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PixelProxy
{
    class TcpProxy
    {
        private String PIXEL_IP = "3.220.252.91";
        private int PIXEL_PORT = 10001;

        private TcpListener Listener;
        private TcpClient Server;

        public TcpProxy()
        {
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), PIXEL_PORT);
        }

        public async void Start()
        {
            Listener.Start();
            await AcceptConnections();
        }

        private async Task AcceptConnections()
        {
            while(true)
            {
                HandleConnection(await Listener.AcceptTcpClientAsync());
            }
        }

        private void HandleConnection(TcpClient client)
        {
            Server = new TcpClient(PIXEL_IP, PIXEL_PORT);
            Console.WriteLine("Connection : " + PIXEL_IP);

            NetworkStream serverStream = Server.GetStream();
            NetworkStream clientStream = client.GetStream();

            new Task(() => OnClientPacket(client, clientStream, serverStream)).Start();
            new Task(() => OnServerPacket(client, serverStream, clientStream)).Start();
        }

        private byte[] OnPacket(byte[] revBuffer, String from)
        {
            // Remove padding and load the bson.
            byte[] data = new byte[revBuffer.Length - 4];
            Buffer.BlockCopy(revBuffer, 4, data, 0, data.Length);
            BSONObject packets = SimpleBSON.Load(data);

            // Modify the packet?
            Console.WriteLine(from + " ========================================================================================");
            for (int i = 0; i < packets["mc"]; i++)
            {
                BSONObject packet = packets["m" + i] as BSONObject;
                ReadBSON(packet);

                if (packet["ID"].stringValue == "OoIP")
                {
                    PIXEL_IP = (packet["IP"].stringValue == "prod.gamev70.portalworldsgame.com" ? "3.220.252.91" : packet["IP"].stringValue);
                    packet["IP"] = "prod.gamev70.portalworldsgame.com";
                }
            }

            // Dump the BSON and add padding.
            MemoryStream memoryStream = new MemoryStream();
            using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
            {
                byte[] bsonDump = SimpleBSON.Dump(packets);

                binaryWriter.Write(bsonDump.Length + 4);
                binaryWriter.Write(bsonDump);
            }
            return memoryStream.ToArray();
        }

        public void ReadBSON(BSONObject SinglePacket, string Parent = "")
        {
            foreach (string Key in SinglePacket.Keys)
            {
                try
                {
                    BSONValue Packet = SinglePacket[Key];

                    switch (Packet.valueType)
                    {
                        case BSONValue.ValueType.String:
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType} = {Packet.stringValue}");
                            break;
                        case BSONValue.ValueType.Boolean:
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType} = {Packet.boolValue}");
                            break;
                        case BSONValue.ValueType.Int32:
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType} = {Packet.int32Value}");
                            break;
                        case BSONValue.ValueType.Int64:
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType} = {Packet.int64Value}");
                            break;
                        case BSONValue.ValueType.Binary: // BSONObject
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType}");
                            ReadBSON(SimpleBSON.Load(Packet.binaryValue), Key);
                            break;
                        case BSONValue.ValueType.Double:
                            Console.WriteLine($"{Parent} = {Key} | {Packet.valueType} = {Packet.doubleValue}");
                            break;
                        default:
                            Console.WriteLine($"{Parent} = {Key} = {Packet.valueType}");
                            break;
                    }

                }
                catch { }
            }
        }

        private void OnClientPacket(TcpClient client, NetworkStream clientStream, NetworkStream serverStream)
        {
            byte[] buffer = new byte[4096];
            int revBytes;

            while (true)
            {
                try
                {
                    revBytes = clientStream.Read(buffer, 0, buffer.Length);
                    if (revBytes <= 0)
                        continue;

                    // TODO: Fix this stupid solution for differentiating from msg packets.
                    try
                    {
                        byte[] newBuffer = OnPacket(buffer, "Client");
                        serverStream.Write(newBuffer, 0, newBuffer.Length);
                    }
                    catch
                    {
                        serverStream.Write(buffer, 0, revBytes);
                    }
                }catch
                {
                    break;
                }
            }
        }

        private void OnServerPacket(TcpClient client, NetworkStream serverStream, NetworkStream clientStream)
        {
            byte[] buffer = new byte[4096];
            int revBytes;

            while (true)
            {
                try
                {
                    revBytes = serverStream.Read(buffer, 0, buffer.Length);
                    if (revBytes <= 0)
                        continue;

                    // TODO: Fix this stupid solution for differentiating from msg packets.
                    try
                    {
                        byte[] newBuffer = OnPacket(buffer, "Server");
                        clientStream.Write(newBuffer, 0, newBuffer.Length);
                    }
                    catch
                    {
                        clientStream.Write(buffer, 0, revBytes);
                    }
                }
                catch
                {
                    break;
                }
            }
        }
    }
}
