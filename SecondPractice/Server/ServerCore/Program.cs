

using System.Net.Sockets;
using System.Net;
using System.Text;

namespace ServerCore
{
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                Session session = new Session();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome");
                session.Send(sendBuff);

                Thread.Sleep(1000);
                session.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry iPHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = iPHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddress, 7777);

            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine("Listening...");

            while (true)
            {
                //
            }
        }
    }
}
