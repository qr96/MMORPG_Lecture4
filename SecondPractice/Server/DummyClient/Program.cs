

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = iPHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddress, 7777);

            while (true)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint}");

                    for (int i = 0; i < 5; i++)
                    {
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World {i}");
                        int sendBytes = socket.Send(sendBuff);
                    }

                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff);
                    Console.WriteLine($"[From Server] {recvData}");

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
