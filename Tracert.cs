using System.Net;
using System.Net.Sockets;

namespace TRC
{
    class Traceroute
    {
        Socket socketUDP  = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket socketICMP = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        IPEndPoint? RemotePoint;
        int TTL = 1;

        void Sending()
        {
            byte[] buffer = new byte[512];//буфер для получаемых данных
            Span<byte> span;
            EndPoint endPoint = RemotePoint;
            int port = 33433, routernumb = 1;
            
            DateTime Time;
            TimeSpan TS;

            do
            {
                Console.Write(String.Format("  {0,2}   ", routernumb++));
                for (int i = 0; i < 3; i++)
                {   
                    socketUDP.SendTo(new byte[1], new IPEndPoint(RemotePoint.Address, ++port));
                    Time = DateTime.Now;
                    try
                    {
                        do
                        {
                            socketICMP.ReceiveFrom(buffer, ref endPoint);
                            span = buffer.AsSpan()[50..52];
                            span.Reverse();
                        } while (BitConverter.ToUInt16(span) != port);
                        TS = DateTime.Now - Time;
                        Console.Write(String.Format("{0,3} ms ", TS.Milliseconds));
                    }
                    catch (Exception)
                    {
                        Console.Write(String.Format("{0,3}    ", "*"));
                    }
                }
                Console.WriteLine($" {((IPEndPoint)endPoint).Address.ToString()}");

                socketUDP.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ++TTL);
            } while (TTL <= 30 && RemotePoint.Address.ToString() != ((IPEndPoint)endPoint).Address.ToString());

            Console.WriteLine("\nТрассировка завершена.");
        }

        void Preparing()
        {
            try
            {
                IPHostEntry? IPHE = Dns.GetHostEntry(Console.ReadLine());
                RemotePoint = new IPEndPoint(IPHE.AddressList[0], 33434); //Для трассировки по udp принято устанавливать 33434 порт
                
                socketUDP.Connect(RemotePoint);//Делается для выбора корректного сетевого адаптера 
                socketUDP.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, TTL);
                socketICMP.Bind(socketUDP.LocalEndPoint);
                socketICMP.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });
                socketICMP.ReceiveTimeout = 4000;
                
                Console.WriteLine($"\nТрассировка маршрута к {IPHE.HostName} [{IPHE.AddressList[0]}]\nс максималным количество прыжков 30:\n");
            }
            catch (System.Exception)
            {
                Console.WriteLine("SOCKETS PREPARING ERROR!\n");
            }
        }

        public void Tracing()
        {
            Preparing();
            Sending();
        }
    }
}
