using DummyClient;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    ServerSession _session = new ServerSession();

    void Start()
    {
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () => { return _session; }, 1);

        StartCoroutine(SendCo());
    }

    private void Update()
    {
        IPacket packet = PacketQueue.Instance.Pop();
        if (packet != null)
        {
            PacketManager.Instance.HandlePacket(_session, packet);
        }
    }

    IEnumerator SendCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            C_Chat chat = new C_Chat();
            chat.chat = "Hello Unity";
            ArraySegment<byte> seg = chat.Write();

            _session.Send(seg);
        }
    }
}
