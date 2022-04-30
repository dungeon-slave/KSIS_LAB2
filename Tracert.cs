using System.Net;
using System.Net.Sockets;

namespace Trcrt
{
    class Tracert
    {
        Socket socketUDP = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket socketICMP = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        EndPoint? RemotePoint;
        IPHostEntry? GetDestinationIp()
        {
            string? DestinationName = Console.ReadLine();
            if (DestinationName != null)
            {
                return Dns.GetHostEntry(DestinationName);
            }
            return null;
        }
        byte[] SendPackets()
        {
            byte[] buffer = new byte[256];//буфер для получаемых данных
            if(RemotePoint != null)
            {
                //socketICMP.BeginReceive(buffer, 0 , 0, 0, new AsyncCallback(CHNG), null);
                // for(int i = 0; i < 3; i++)
                // { 
                socketUDP.SendTo(new byte[1], RemotePoint);
                socketICMP.Receive(buffer);
                Span<byte> span = buffer.AsSpan()[50..52];
                span.Reverse();
                Console.WriteLine(BitConverter.ToUInt16(span));
                // }
                return buffer;
            }
            return buffer;
        }

        // void CHNG(IAsyncResult ar)
        // {
        //     socketICMP.EndReceive(ar);
        // }
        public void Test()
        {
            IPHostEntry? IPHE = GetDestinationIp();
            try
            {
                if (IPHE != null)
                {
                    RemotePoint = new IPEndPoint(IPHE.AddressList[0], 33434); //Для трассировки по udp принято устанавливать 33434 порт
                    Console.WriteLine($"Трассировка маршрута к {IPHE.HostName} [{IPHE.AddressList[0]}]");
 
                    socketUDP.Connect(RemotePoint);//Делается для выбора корректного сетевого адаптера 
                    socketICMP.Bind(socketUDP.LocalEndPoint);
                    Console.WriteLine(socketUDP.LocalEndPoint);
                    socketICMP.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });
                    
                    SendPackets();
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Socket error!");
            }
        }
    }
}