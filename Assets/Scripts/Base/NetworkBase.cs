using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Services
{
    public class NetworkBase : MonoBehaviour
    {
        private static TcpClient _tcpClient;
        private static NetworkStream _networkStream = null;
        private static NetworkBase _networkBase;
        protected const int WaitingMessageFrequency = 2;
        protected delegate void OnMessageReceived(string message);
        protected event OnMessageReceived OnMessageReceivedEvent;
        
        
        private void Awake()
        {
            if (_networkBase == null)
            {
                _networkBase = this;
                _tcpClient = new TcpClient();
                ConnectToServerApi("127.0.0.1", 5050);
            }
            else
            {
                Debug.unityLogger.Log("Instance NetworkBase already exists. Destroying it");
                Destroy(this);
            }
        }

        private void Update()
        {
        }

        protected void ConnectToServerApi(string ip, int port)
        {
            try
            {
                _tcpClient.Connect(ip, port);
                var t = new Thread(new ThreadStart(ListenForUpdates));
                t.IsBackground = true;
                t.Start();
            }
            catch (SocketException e)
            {
                Debug.unityLogger.LogException(e);
                Disconnect();
            }
        }

        private void ListenForUpdates()
        {
            while (true)
            {
                _networkStream = _tcpClient.GetStream();
                int length;
                do
                {
                    byte[] encodedMessage = new byte[128];
                    length = _networkStream.Read(encodedMessage, 0, encodedMessage.Length);
                    var messageLength = encodedMessage.Length;

                    string message = Encoding.ASCII.GetString(encodedMessage, 0, messageLength);
                    OnMessageReceivedEvent?.Invoke(message);
                } while (length != 0);
            }
        }


        public static void SendMessageToServer(string message)
        {
            if (_networkStream == null)
            {
                _networkStream = _tcpClient.GetStream();
            }
            
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            _networkStream.Write(buffer, 0, buffer.Length);
        }
        
        
        
        protected void Disconnect()
        {
            if (_tcpClient.Connected)
            {
                _tcpClient.Close();
                _networkStream.Close();
            }
            if (_tcpClient != null) _tcpClient = null;
        }
    }
}