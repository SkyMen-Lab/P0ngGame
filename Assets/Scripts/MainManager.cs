using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Models;
using Newtonsoft.Json;
using Services;
using UnityEngine;
using UnityEngine.UI;

public class MainManager : MonoBehaviour
{
    public string ipAdress;
    public int port;
    
    private UIService _ui;
    private NetworkManager _networkManager;
    private BallController _ballController;
    private readonly TeamRepository _teamRepository = TeamRepository.GetTeamRepository();

    #region Timers

    private Timer _connectionTimer;
    

    #endregion

    private bool _isConnected;

    private void Awake()
    {
        _ui = GetComponent<UIService>();
        _networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        _connectionTimer = new Timer(5000);
        _connectionTimer.Elapsed += ConnectionTimerOnElapsed;
        _connectionTimer.AutoReset = false;
        
        //Ball instance can be accessed only on Start 
        _ballController = BallController.Instance;
        _ballController.OnBallScoredEvent += ProcessScore;
        _networkManager.OnServerConnectedEvent += ConnectToServerHandler;
        _networkManager.OnServerDisconnected += DisconnectFromServerHandler;
        _networkManager.OnTeamReceivedEvent += TeamReceived;
        _networkManager.OnStartedGameEvent += StartMovingBall;
        _networkManager.OnMovedPaddleEvent += MovePaddle;

        _isConnected = _networkManager.Connect(ipAdress, port);

        foreach (var label in GameObject.FindGameObjectsWithTag("Team Label"))
        {
            label.GetComponent<Text>().text = string.Empty;
        }
    }

    private void ConnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        Debug.LogWarning("Server is offline. Reconnecting");
        _networkManager.Connect(ipAdress, port);
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        _networkManager.OnServerConnectedEvent -= ConnectToServerHandler;
        _networkManager.OnServerDisconnected -= DisconnectFromServerHandler;
        _networkManager.OnTeamReceivedEvent -= TeamReceived;
        _networkManager.OnStartedGameEvent -= StartMovingBall;
        _networkManager.OnMovedPaddleEvent -= MovePaddle;

        _ballController.OnBallScoredEvent -= ProcessScore;
    }

    private void ConnectToServerHandler()
    {
        _ui.status.text = "Awaiting Teams";
    }

    private void DisconnectFromServerHandler()
    {
        _ui.status.text = "Server is unavailable";
        Reconnect();
    }

    private void Reconnect()
    {
        if (!_networkManager.IsConnected())
        {
            _connectionTimer?.Start();
        }
        else
        {
            _connectionTimer?.Stop();
            Debug.Log("Back online");
        }
    }

    private void TeamReceived(List<Team> teams)
    {
        teams[0].Side = Side.Right;
        GameObject.Find("Paddle Right").GetComponent<PaddleController>().TeamCode = teams[0].Code;
        
        teams[1].Side = Side.Left;
        GameObject.Find("Paddle Left").GetComponent<PaddleController>().TeamCode = teams[1].Code;
        
        _teamRepository.Init(teams);

        _ui.status.text = string.Empty;
        _ui.firstTeamLabel.text = teams[0].Name;
        _ui.firstTeamScore.text = teams[0].Score.ToString();
        
        _ui.secondTeamLabel.text = teams[1].Name;
        _ui.secondTeamScore.text = teams[1].Score.ToString();
    }

    private void MovePaddle(KeyValuePair<string, float> moveContext)
    {
        var team = _teamRepository.FindTeamByCode(moveContext.Key);
        if (team == null) return;
        var side = team.Side;
        GameObject paddle;
        if (side == Side.Right)
            paddle = GameObject.Find("Paddle Right");
        else paddle = GameObject.Find("Paddle Left");
        
        paddle.GetComponent<PaddleController>().HandleClick(moveContext.Value);
    }

    private void ProcessScore(GameObject zone)
    {

        Side side;
        Team team;
        if (zone.name == "Left")
        {
            side = Side.Right;
            _teamRepository.IncrementScore(side);
            team = _teamRepository.FindTeam(x => x.Side == side);
            _ui.firstTeamScore.text = team.Score.ToString();
        }
        else
        {
            side = Side.Left;
            _teamRepository.IncrementScore(side);
            team = _teamRepository.FindTeam(x => x.Side == side);
            _ui.secondTeamScore.text = team.Score.ToString();
        }

        var message = new Message
        {
            ContentType = GameAction.Score,
            Content = JsonConvert.SerializeObject(team)
        };
        
        var packet = new Packet(Meta.Message, JsonConvert.SerializeObject(message));
        _networkManager.SendPacketToServer(packet);
    }

    private void FinishGame()
    {
        _ballController.StopTheBall();

        var teams = _teamRepository.GetList();

        if (teams.Count == 2)
        {
            var packet = new Packet(Meta.Disconnect, JsonConvert.SerializeObject(teams));
            _networkManager.SendPacketToServer(packet);
        }
    }

    private void StartMovingBall()
    {
        _ballController.ResetBall(BallController.StartDirection.Left);
        Debug.Log("Ball started moving");
    }
}