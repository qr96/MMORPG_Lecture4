using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _sendLock = new object();
        Queue<byte[]> _sendQue = new Queue<byte[]>();
        bool _sendPending = false;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvComplete);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendComplete);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            lock (_sendLock)
            {
                _sendQue.Enqueue(sendBuff);
                if (_sendPending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void RegisterSend()
        {
            _sendPending = true;
            byte[] sendBuff = _sendQue.Dequeue();
            _sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendComplete(null, _sendArgs);
        }

        void OnSendComplete(object sender, SocketAsyncEventArgs args)
        {
            lock (_sendLock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQue.Count > 0)
                            RegisterSend();
                        else
                            _sendPending = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendComplete Failed {ex}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvComplete(null, args);
        }

        void OnRecvComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");
                    RegisterRecv(args);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"OnRecvComplete Failed {ex}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }
}
