using System;
using System.Collections;
using Models;
using Services;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    private UIService _ui;
    private NetworkManager _networkManager;
    private BallController _ballController;

    private bool _isConnected;

    private void Awake()
    {
        _ui = GetComponent<UIService>();
        _networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        _ballController = BallController.Instance;
        _isConnected = _networkManager.Connect("127.0.0.1", 5050);
    }

    private void OnEnable()
    {
        _networkManager.OnServerConnectedEvent += ConnectToServerHandler;
        _networkManager.OnServerDisconnected += DisconnectFromServerHandler;
    }
    
    
    IEnumerator TryConnect()
    {
        _networkManager.Connect("127.0.0.1", 5050);
        yield return new WaitForSeconds(4.0f);
    }

    private void OnDisable()
    {
        _networkManager.OnServerConnectedEvent -= ConnectToServerHandler;
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

    private void StartMovingBall()
    {
        _ballController.ResetBall(BallController.StartDirection.Left);
        Debug.Log("Ball started moving");
    }
    
    
    
}