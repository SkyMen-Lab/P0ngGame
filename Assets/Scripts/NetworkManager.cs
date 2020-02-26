using System;
using System.Collections;
using System.Threading;
using System.Timers;
using Base;
using Models;
using Services;
using UnityEngine;
using UnityEngine.UI;
using Timer = System.Timers.Timer;

public class NetworkManager : NetworkBase
{
    private UIService _uiService;
    public static NetworkManager Instance;

    protected override void AwakeSetup()
    {
        _uiService = GetComponent<UIService>();
    }

    public bool Connect(string ip, int port)
    {
        ConnectToServerApi(ip, port);
        return IsConnected();
    }
}