using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DTOs;
using Mirror;
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
    //singleton
    private readonly TeamRepository _teamRepository = TeamRepository.GetTeamRepository();

    #region Timers

    private Timer _connectionTimer;
    

    #endregion

    private bool _isConnected;

    #region constants
    

    #endregion

    private void Awake()
    {
        _ui = GetComponent<UIService>();
        _networkManager = GetComponent<NetworkManager>();
    }

    private async void Start()
    {
        _connectionTimer = new Timer(5000);
        _connectionTimer.Elapsed += CheckConnectionTimerOnElapsed;
        _connectionTimer.AutoReset = true;
        _connectionTimer.Start();
        
        //Ball instance can be accessed only on Start 
        _ballController = BallController.Instance;
        _ballController.OnBallScoredEvent += ProcessScore;
        _networkManager.OnServerConnectedEvent += ConnectToServerHandler;
        _networkManager.OnServerDisconnectedEvent += DisconnectFromServerHandler;
        _networkManager.OnConfigReceivedEvent += ConfigReceived;
        _networkManager.OnStartedGameEvent += StartMovingBall;
        _networkManager.OnMovedPaddleEvent += MovePaddle;
        _networkManager.OnUpdateNumberOfPlayersEvent += UpdateNumberOfPlayers;
        _networkManager.OnResetServerEvent += ResetServer;

         _networkManager.ConnectToServerApi(ipAdress, port);

         _ui.finishGameBtn.onClick.AddListener(FinishGame);

        foreach (var label in GameObject.FindGameObjectsWithTag("Team Label"))
        {
            label.GetComponent<Text>().text = string.Empty;
        }
        
    }

    private void CheckConnectionTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        var isConnected = _networkManager.CheckConnection();
        if (!isConnected)
        {
            Debug.LogWarning("Server is offline. Reconnecting");
            _networkManager.ConnectToServerApi(ipAdress, port);
        }
    }

    private void OnDisable()
    {
        _networkManager.OnServerConnectedEvent -= ConnectToServerHandler;
        _networkManager.OnServerDisconnectedEvent -= DisconnectFromServerHandler;
        _networkManager.OnConfigReceivedEvent -= ConfigReceived;
        _networkManager.OnStartedGameEvent -= StartMovingBall;
        _networkManager.OnMovedPaddleEvent -= MovePaddle;
        _networkManager.OnUpdateNumberOfPlayersEvent -= UpdateNumberOfPlayers;
        _networkManager.OnResetServerEvent -= ResetServer;

        _ballController.OnBallScoredEvent -= ProcessScore;
        
        _connectionTimer.Stop();
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

    private IEnumerator RunGameTimer()
    {
        float time = Config.GetConfig().Duration * 60;

        while (time >= 0)
        {
            time -= Time.deltaTime;
            int mins = (int) (time / 60);
            int secs = (int)time - mins * 60;
            _ui.timer.text = $"{mins}:{secs:00}";
            yield return null;
        } 
        
        FinishGame();
    }

    private void ConfigReceived(GameSetupDTO dto)
    {
        var teams = dto.Teams;
        float duration = dto.Duration;
        
        teams[0].Side = Side.Left;
        GameObject.Find("Paddle Right").GetComponent<PaddleController>().TeamCode = teams[0].Code;
        
        teams[1].Side = Side.Right;
        GameObject.Find("Paddle Left").GetComponent<PaddleController>().TeamCode = teams[1].Code;
        
        _teamRepository.Init(teams);

        var config = Config.GetConfig();
        config.SetupConfig(dto.Code, dto.Duration);

        _ui.status.text = string.Empty;
        _ui.firstTeamLabel.text = teams[0].Name;
        _ui.firstTeamScore.text = teams[0].Score.ToString();
        
        _ui.secondTeamLabel.text = teams[1].Name;
        _ui.secondTeamScore.text = teams[1].Score.ToString();

        _ui.teamOnePlayers.text = $"<b>{teams[0].NumberOfPlayers}</b> in game";
        _ui.teamTwoPlayers.text = $"<b>{teams[1].NumberOfPlayers}</b> in game";

        _ui.timer.text = duration.ToString("F2");
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

    private void UpdateNumberOfPlayers(string code, int numberOfPlayers)
    {
        var team = _teamRepository.FindTeamByCode(code);
        team.NumberOfPlayers = numberOfPlayers;
        _teamRepository.UpdateTeam(team);
        
        var msg = $"<b>{team.NumberOfPlayers}</b> in game";

        if (team.Side == Side.Left)
        {
            _ui.teamOnePlayers.text = msg;
        }
        else
        {
            _ui.teamTwoPlayers.text = msg;
        }
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
            _ui.secondTeamScore.text = team.Score.ToString();
        }
        else
        {
            side = Side.Left;
            _teamRepository.IncrementScore(side);
            team = _teamRepository.FindTeam(x => x.Side == side);
            _ui.firstTeamScore.text = team.Score.ToString();
        }

        var message = new Message(GameAction.Score, JsonConvert.SerializeObject(team));

        var packet = new Packet(Meta.Message, message.ToJson());
        _networkManager.SendPacketToServer(packet);
    }

    private void FinishGame()
    {
        Debug.Log("finish game button clicked");
        _ballController.StopTheBall();

        var teams = _teamRepository.GetList();

        if (teams.Count == 2)
        {
            var gameFinishedDTO = new GameFinishedDTO()
            {
                GameCode = Config.GetConfig().GameCode,
                WinnerCode = teams.Aggregate((t1, t2) => t1.Score > t2.Score ? t1 : t2).Code,
                MaxSpeedLevel = 3,
                Teams = teams
            };
            var message = new Message(GameAction.FinishGame, JsonConvert.SerializeObject(gameFinishedDTO));
            var packet = new Packet(Meta.Disconnect, message.ToJson());
            _networkManager.SendPacketToServer(packet);
        }
    }

    private void StartMovingBall()
    {
        _ballController.ResetBall(BallController.StartDirection.Left);
        StartCoroutine(RunGameTimer());
        Debug.Log("Ball started moving");
    }

    private void ResetServer()
    {
        _ui.status.text = "Awaiting Teams";
        
        _ui.firstTeamLabel.text = string.Empty;
        _ui.secondTeamLabel.text = string.Empty;
        
        _ui.teamOnePlayers.text = string.Empty;
        _ui.teamTwoPlayers.text = string.Empty;
        
        _ui.firstTeamScore.text = string.Empty;
        _ui.secondTeamScore.text = string.Empty;

        _ui.timer.text = "0.00";
        
        Config.GetConfig().Reset();
        _teamRepository.Reset();
        
        Debug.Log("Server has been reset");
    }
}