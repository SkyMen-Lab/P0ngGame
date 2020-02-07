using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Models;
using Services;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private UIService _ui;
    private NetworkManager _networkManager;
    private BallController _ballController;
    private Timer _timer;
    private readonly TeamRepository _teamRepository = TeamRepository.GetTeamRepository();

    private bool _isConnected;

    private void Awake()
    {
        _ui = GetComponent<UIService>();
        _networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        _timer = new Timer(5000);
        _timer.Elapsed += TimerOnElapsed;
        _timer.AutoReset = false;
        
        //Ball instance can be accessed only on Start 
        _ballController = BallController.Instance;
        _ballController.OnBallScoredEvent += ProcessScore;
        _networkManager.OnServerConnectedEvent += ConnectToServerHandler;
        _networkManager.OnServerDisconnected += DisconnectFromServerHandler;
        _networkManager.OnTeamReceivedEvent += TeamReceived;
        _networkManager.OnStartedGameEvent += StartMovingBall;
        _networkManager.OnMovedPaddleEvent += MovePaddle;
        _networkManager.OnGameFinishedEvent += FinishGame;
        
        //TODO: Connect in background
        _isConnected = _networkManager.Connect("127.0.0.1", 3434);
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        Debug.LogWarning("Server is offline. Reconnecting");
        _networkManager.Connect("127.0.0.1", 3434);
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
        _networkManager.OnGameFinishedEvent -= FinishGame;

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
            _timer?.Start();
        }
        else
        {
            _timer?.Stop();
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

        _ui.status.text = "";
        _ui.firstTeamLabel.text = teams[0].Name;
        _ui.secondTeamLabel.text = teams[1].Name;
    }

    private void MovePaddle(KeyValuePair<string, float> moveContext)
    {
        var side = _teamRepository.FindTeamByCode(moveContext.Key).Side;
        GameObject paddle;
        if (side == Side.Right)
            paddle = GameObject.Find("Paddle Right");
        else paddle = GameObject.Find("Paddle Left");
        
        paddle.GetComponent<PaddleController>().HandleClick(moveContext.Value);
    }

    private void ProcessScore(GameObject zone)
    {
        //TODO: send updates to server
        
        Side side;
        if (zone.name == "Left")
        {
            side = Side.Right;
            _teamRepository.IncrementScore(side);
            _ui.firstTeamScore.text = _teamRepository.FindTeam(x => x.Side == side).Score.ToString();
        }
        else
        {
            side = Side.Left;
            _teamRepository.IncrementScore(side);
            _ui.firstTeamScore.text = _teamRepository.FindTeam(x => x.Side == side).Score.ToString();
        }
    }

    private void FinishGame()
    {
        //TODO: finish
        _ballController.StopTheBall();
    }

    private void StartMovingBall()
    {
        _ballController.ResetBall(BallController.StartDirection.Left);
        Debug.Log("Ball started moving");
    }
    
    
    
}