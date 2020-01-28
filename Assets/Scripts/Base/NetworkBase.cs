using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Services
{
    public class NetworkBase : MonoBehaviour
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private IEnumerator _listenForUpdatesCoroutine;
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
            }
            else
            {
                Debug.unityLogger.Log("Instance NetworkBase already exists. Destroying it");
                Destroy(this);
            }
        }

        private void Update()
        {
            StartCoroutine(_listenForUpdatesCoroutine);
        }

        protected void ConnectToServerApi(string ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(ip, port);
                _listenForUpdatesCoroutine = ListenForUpdates();

            }
            catch (SocketException e)
            {
                Debug.unityLogger.LogException(e);
                Disconnect();
            }

        }

        private IEnumerator ListenForUpdates()
        {
            _networkStream = _tcpClient.GetStream();
            do
            {
                byte[] encodedMessage = new byte[128];
                _networkStream.Read(encodedMessage, 0, encodedMessage.Length);
                var messageLength = encodedMessage.Length;

                string message = Encoding.ASCII.GetString(encodedMessage, 0, messageLength);
                OnMessageReceivedEvent?.Invoke(message);
                
                yield return new WaitForSeconds(WaitingMessageFrequency);
            } while (true);
        }


        protected void SendMessageToServer(string message)
        {
            if (_networkStream == null)
            {
                return;
            }
            
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            _networkStream.Write(buffer, 0, buffer.Length);
        }
        
        
        
        private void Disconnect()
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