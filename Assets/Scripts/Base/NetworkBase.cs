using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Models;
using UnityEngine;

namespace Services
{
    public class NetworkBase : MonoBehaviour
    {

        #region Instances

        private static TcpClient _tcpClient;
        private static NetworkStream _networkStream = null;
        private static NetworkBase _networkBase;

        #endregion

        #region Events

        
        //TODO: replace with specific events
        public delegate void OnMessageReceived(string message);
        public event OnMessageReceived OnMessageReceivedEvent;

        public delegate void TeamsReceived(List<Team> teams);
        public event TeamsReceived OnTeamReceivedEvent;

        public delegate void StartGame();
        public event StartGame OnStartedGameEvent;

        public delegate void MovePaddle(KeyValuePair<Team, float> movement);
        public event MovePaddle OnMovedPaddleEvent;

        public delegate void FinishGame();
        public event FinishGame OnGameFinishedEvent;

        public delegate void ServerConnected();
        public event ServerConnected OnServerConnectedEvent;

        public delegate void ServerDisconnected();
        public event ServerDisconnected OnServerDisconnected;
        
        #endregion
        
        void Start()
        {
            if (_networkBase == null)
            {
                _networkBase = this;
                StartSetup();
            }
            else
            {
                Debug.unityLogger.Log("Instance NetworkBase already exists. Destroying it");
                Destroy(this);
            }
        }

        protected virtual void StartSetup()
        {
            
        }

        void Awake()
        {
            AwakeSetup();
        }

        private void Update()
        {
            if (!IsConnected())
            {
                OnServerDisconnected?.Invoke();
            }
        }

        protected virtual void AwakeSetup()
        {
            
        }

        protected void ConnectToServerApi(string ip, int port)
        {
            try
            {
                if (_tcpClient == null)
                    _tcpClient = new TcpClient();
                _tcpClient.Connect(ip, port);
                var t = new Thread(ListenForUpdates);
                t.IsBackground = true;
                t.Start();
            }
            catch (SocketException e)
            {
                Debug.unityLogger.LogException(e);
                Disconnect();
            }

            if (IsConnected())
            {
                OnServerConnectedEvent?.Invoke();
            }
        }

        protected bool IsConnected()
        {
            return _tcpClient != null && _tcpClient.Connected;
        }

        private void ListenForUpdates()
        {
            while (IsConnected())
            {
                _networkStream = _tcpClient.GetStream();
                int length;
                do
                {
                    byte[] encodedMessage = new byte[128];
                    length = _networkStream.Read(encodedMessage, 0, encodedMessage.Length);
                    var messageLength = encodedMessage.Length;
                    string message = Encoding.ASCII.GetString(encodedMessage, 0, messageLength);

                    var msgType = MessageHandler.ProcessMessageType(message);
                    if (msgType == null) continue;

                    switch (msgType)
                    {
                        case MessageHandler.MessageType.StartGame:
                            OnStartedGameEvent?.Invoke();
                            break;
                        
                        case MessageHandler.MessageType.FinishGame:
                            OnGameFinishedEvent?.Invoke();
                            break;
                        
                        
                        default:
                            break;
                    }
                    
                    OnMessageReceivedEvent?.Invoke(message);
                } while (length != 0);
            }
        }


        public virtual void SendMessageToServer(string message)
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
            OnServerDisconnected?.Invoke();
        }
    }
}