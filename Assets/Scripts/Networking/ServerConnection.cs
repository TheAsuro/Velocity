using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Networking
{
    public class ServerConnection
    {
        public delegate void OnConnected();

        private OnConnected Connected;
        private Thread listenThread;

        private TcpClient client;
        private BinaryReader reader;
        private BinaryWriter writer;
        private bool continueConnection = true;
        private bool connected = false;

        private string ip;

        private int port;
        //string password;

        public ServerConnection(string serverIp, int serverPort, string serverPass = "")
        {
            ip = serverIp;
            port = serverPort;
            //password = serverPass;
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <returns>returns -1 if can't parse IP, 0 if other error and 1 if connected</returns>
        public int Connect(OnConnected connectedCall)
        {
            try
            {
                client = new TcpClient(ip, port);
                reader = new BinaryReader(client.GetStream());
                writer = new BinaryWriter(client.GetStream());
                Connected = connectedCall;
                listenThread = new Thread(new ThreadStart(Listen));
                listenThread.Start();
                return 1;
            }
            catch(System.Exception ex)
            {
                Debug.Log(ex.Message);
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
}
