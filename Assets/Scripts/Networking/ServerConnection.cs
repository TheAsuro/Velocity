using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

public class ServerConnection
{
    public delegate void OnConnected();
    OnConnected Connected;
    Thread listenThread;

    TcpClient client;
    BinaryReader reader;
    BinaryWriter writer;
    bool continueConnection = true;
    bool connected = false;

    string ip;
    int port;
    string password;

    public ServerConnection(string serverIp, int serverPort, string serverPass = "")
    {
        ip = serverIp;
        port = serverPort;
        password = serverPass;
    }

    /// <summary>
    /// Connect
    /// </summary>
    /// <returns>returns -1 if can't parse IP, 0 if other error and 1 if connected</returns>
    public int Connect(OnConnected ConnectedCall)
    {
        try
        {
            client = new TcpClient(ip, port);
            reader = new BinaryReader(client.GetStream());
            writer = new BinaryWriter(client.GetStream());
            Connected = ConnectedCall;
            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
            return 1;
        }
        catch(System.Exception ex)
        {
            return 0;
        }
    }

    private void Listen()
    {
        while(continueConnection)
        {
            if(!connected)
            {
                Connected();
                connected = true;
            }

            int info = reader.ReadByte();
            if(info >= 0 && info <= 255)
            {
                if (info == 5)
                    writer.Write((byte)5);
                Debug.Log("Got byte " + info.ToString());
            }
            else
            {
                continueConnection = false;
                Debug.Log("ReadByte returned error");
            }
        }
    }

    public void Disconnect()
    {
        client.Close();
    }
}
