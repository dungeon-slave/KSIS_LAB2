using System.Net;
using System.Net.Sockets;

namespace TRC
{
    class Traceroute
    {
        Socket      socketUDP  = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        Socket      socketICMP = new(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        IPEndPoint? remotePoint;
        int         ttl = 1;
        int         port = 33434;

        bool PrepareSockets()
        {
            try
            {
                IPHostEntry? iphe = Dns.GetHostEntry(Console.ReadLine());
                remotePoint = new(iphe.AddressList[0], port); //Для трассировки по udp принято устанавливать 33434 порт
                
                socketUDP.Connect(remotePoint);//Делается для выбора корректного
                socketICMP.Bind(socketUDP.LocalEndPoint);//сетевого адаптера
                socketICMP.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });
                socketICMP.ReceiveTimeout = 4000;
                
                Console.WriteLine($"\nТрассировка маршрута к {iphe.HostName} [{iphe.AddressList[0]}]\nс максималным количество прыжков 30:\n");
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("SOCKETS PREPARING ERROR!\n");
                return false;
            }
        }

        public void TraceRoute()
        {
            if(!PrepareSockets()) { return; }

            DateTime   time;
            TimeSpan   timeSpan;
            Span<byte> span;
            EndPoint   endPoint = remotePoint;
            int        routernumb = 1;
            byte[]     buffer = new byte[128];//буфер для получаемых данных
            bool       success;

            do
            {
                socketUDP.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl++);
                Console.Write(String.Format("  {0,2}   ", routernumb++));
                success = false;
                for (int i = 0; i < 3; i++, port++)
                {   
                    socketUDP.SendTo(new byte[1], new IPEndPoint(remotePoint.Address, port));
                    time = DateTime.Now;
 
                    try
                    {

                        do
                        {
                            socketICMP.ReceiveFrom(buffer, ref endPoint);
                            span = buffer.AsSpan()[50..52];
                            span.Reverse();
                        } while (BitConverter.ToUInt16(span) != port);

                        success = true;
                        timeSpan = DateTime.Now - time;
                        Console.Write(String.Format("{0,3} ms ", timeSpan.Milliseconds));
                    }
                    catch (Exception)
                    {
                        Console.Write(String.Format("{0,3}    ", "*"));
                    }
                }

                if(success)
                {
                    Console.WriteLine($" {((IPEndPoint)endPoint).Address.ToString()}");
                }
                else
                {
                    Console.WriteLine(" Превышен интервал ожидания для запроса.");
                }
            } while (ttl <= 30 && !remotePoint.Address.Equals(((IPEndPoint)endPoint).Address));

            Console.WriteLine("\nТрассировка завершена.");
        }
    }
}
