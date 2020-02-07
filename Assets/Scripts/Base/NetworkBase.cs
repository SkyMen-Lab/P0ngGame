using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Models;
using UnityEngine;

namespace Base
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

        public delegate void MovePaddle(KeyValuePair<string, float> movement);
        public event MovePaddle OnMovedPaddleEvent;

        public delegate void FinishGame();
        public event FinishGame OnGameFinishedEvent;

        public delegate void ServerConnected();
        public event ServerConnected OnServerConnectedEvent;

        public delegate void ServerDisconnected();
        public event ServerDisconnected OnServerDisconnected;
        
        #endregion

        //TODO: fucking thread safe
        private string _message;

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

        private void FixedUpdate()
        {
            if (!IsConnected())
            {
                OnServerDisconnected?.Invoke();
            }
            else
            {
                RaiseEvent();
            }
        }

        protected virtual void AwakeSetup()
        {
            
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        protected async void ConnectToServerApi(string ip, int port)
        {
            try
            {
                if (!IsConnected())
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(ip, port);
                    new Task(ListenForUpdates, TaskCreationOptions.LongRunning).Start();
                }
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

        public bool IsConnected()
        {
            return _tcpClient != null && _tcpClient.Connected;
        }

        private void ListenForUpdates()
        {
            try
            {
                if (IsConnected())
                {
                    _networkStream = _tcpClient.GetStream();
                    SendMessageToServer("game");
                    int length;
                    do
                    {
                        byte[] encodedMessage = new byte[512];
                        length = _networkStream.Read(encodedMessage, 0, encodedMessage.Length);
                        var messageLength = encodedMessage.Length;
                        _message = Encoding.ASCII.GetString(encodedMessage, 0, messageLength).Replace(" ", "");
                    } while (length != 0);
                }
            }
            catch (Exception)
            {
                Debug.LogWarning("Connection lost");
            }
        }

        private void RaiseEvent()
        {
            var msgType = MessageHandler.ProcessMessageType(_message);
            if (msgType == null) return;
            Debug.Log("Message received");

            switch (msgType)
            {
                case MessageHandler.MessageType.StartGame:
                    OnStartedGameEvent?.Invoke();
                    break;
                        
                case MessageHandler.MessageType.FinishGame:
                    OnGameFinishedEvent?.Invoke();
                    break;
                        
                case MessageHandler.MessageType.InitTeams:
                    var teams = MessageHandler.ParseTeams(_message);
                    if (teams != null)
                    {
                        OnTeamReceivedEvent?.Invoke(teams);
                    }
                    break;
                        
                case MessageHandler.MessageType.Movement:
                    var movement = MessageHandler.ParseMovement(_message);
                    if (movement != null) OnMovedPaddleEvent?.Invoke((KeyValuePair<string, float>) movement);
                    break;
            }
            OnMessageReceivedEvent?.Invoke(_message);
            _message = "";
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
            if (IsConnected())
            {
                _tcpClient.Close();
                _networkStream.Close();
            }
            _tcpClient = null;
            OnServerDisconnected?.Invoke();
        }
    }
}