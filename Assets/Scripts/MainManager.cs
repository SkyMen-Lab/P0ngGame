using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using Services;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private UIService _ui;
    private NetworkManager _networkManager;
    private BallController _ballController;
    private readonly TeamRepository _teamRepository = TeamRepository.GetTeamRepository();

    private bool _isConnected;

    private void Awake()
    {
        _ui = GetComponent<UIService>();
        _networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
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
        _isConnected = _networkManager.Connect("127.0.0.1", 5050);
    }

    private void OnEnable()
    {

    }
    

    
    
    IEnumerator TryConnect()
    {
        _networkManager.Connect("127.0.0.1", 3434);
        yield return new WaitForSeconds(4.0f);
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
        StopCoroutine(TryConnect());
    }

    private void DisconnectFromServerHandler()
    {
        _ui.status.text = "Server is unavailable";
        StartCoroutine(TryConnect());
    }

    private void TeamReceived(List<Team> teams)
    {
        teams[0].Side = Side.Right;
        GameObject.FindWithTag("addle Right").GetComponent<PaddleController>().TeamCode = teams[0].Code;
        
        teams[1].Side = Side.Left;
        GameObject.FindWithTag("Paddle Left").GetComponent<PaddleController>().TeamCode = teams[1].Code;
        
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