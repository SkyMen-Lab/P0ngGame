using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DTOs;
using Models;
using Newtonsoft.Json;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{

    #region Instances

    private static TcpClient _tcpClient;
    private static NetworkStream _networkStream = null;
    private static NetworkManager _networkManager;

    #endregion

    #region Events

        
    public delegate void OnMessageReceived(string message);
    public event OnMessageReceived OnMessageReceivedEvent;

    public delegate void TeamsReceived(GameSetupDTO dto);
    public event TeamsReceived OnConfigReceivedEvent;

    public delegate void StartGame();
    public event StartGame OnStartedGameEvent;

    public delegate void MovePaddle(KeyValuePair<string, float> movement);
    public event MovePaddle OnMovedPaddleEvent;

    public delegate void UpdateNumberOfPlayers(string code, int numberOfPlayers);

    public event UpdateNumberOfPlayers OnUpdateNumberOfPlayersEvent;

    public delegate void ServerConnected();
    public event ServerConnected OnServerConnectedEvent;

    public delegate void ServerDisconnected();
    public event ServerDisconnected OnServerDisconnectedEvent;

    public delegate void ResetServer();
    public event ResetServer OnResetServerEvent;
        
    #endregion

    //TODO: fucking thread safe
    private string _message;

    void Start()
    {
        if (_networkManager == null)
        {
            _networkManager = this;
            StartSetup();
        }
        else
        {
            Debug.unityLogger.Log("Instance NetworkManager already exists. Destroying it");
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
            OnServerDisconnectedEvent?.Invoke();
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

    public async Task ConnectToServerApi(string ip, int port)
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
        catch (SocketException)
        {
            Debug.LogWarning("Error connecting");
            Disconnect();
        }

        if (IsConnected())
        {
            OnServerConnectedEvent?.Invoke();
        }
    }

    public bool IsConnected() => _tcpClient != null && _tcpClient.Connected;

    public bool CheckConnection()
    {
        if (IsConnected())
        {
            try
            {
                _tcpClient.Client.Send(new byte[1], 1, SocketFlags.None);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        return false;
    }

        private void ListenForUpdates()
    {
        try
        {
            if (IsConnected())
            {
                _networkStream = _tcpClient.GetStream();
                SendPacketToServer(new Packet(Meta.Connect, "game"));
                int length;
                do
                {
                    byte[] encodedMessage = new byte[1024];
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
        if (string.IsNullOrEmpty(_message)) return;
        var packet = Packet.FromJson(_message);
        var message = Message.FromJson(packet.Message);
        Debug.Log("Message received");

        switch (message.ContentType)
        {
            case GameAction.StartGame:
                OnStartedGameEvent?.Invoke();
                break;

            case GameAction.InitGame:
                var game = JsonConvert.DeserializeObject<GameSetupDTO>(message.Content);
                if (game != null)
                {
                    OnConfigReceivedEvent?.Invoke(game);
                }
                break;
                
            case GameAction.Movement:
                var movement = MessageHandler.ParseMovement(message.Content);
                OnMovedPaddleEvent?.Invoke(movement);
                break;
            
            case GameAction.UpdateNumberOfPlayers:
                var dto = JsonConvert.DeserializeObject<PlayersUpdateDTO>(message.Content);
                OnUpdateNumberOfPlayersEvent?.Invoke(dto.TeamCode, dto.NumberOfPlayers);
                break;
            
            case GameAction.FinishGame:
                OnResetServerEvent?.Invoke();
                break;
                
        }
        OnMessageReceivedEvent?.Invoke(_message);
        _message = "";
    }


    public void SendPacketToServer(Packet packet)
    {
        if (_networkStream == null)
        {
            _networkStream = _tcpClient.GetStream();
        }
            
        byte[] buffer = Encoding.ASCII.GetBytes(packet.ToJson());
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
        OnServerDisconnectedEvent?.Invoke();
    }
}