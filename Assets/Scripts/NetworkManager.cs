using System;
using System.Collections;
using Services;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : NetworkBase
{
    private UIService _uiService;
    public static NetworkManager Instance;

    protected override void AwakeSetup()
    {
        _uiService = GetComponent<UIService>();
    }

    public override void SendMessageToServer(string message)
    {
        if (!string.IsNullOrEmpty(message))
            base.SendMessageToServer(message);
    }

    public bool Connect(string ip, int port)
    {
        ConnectToServerApi(ip, port);
        return IsConnected();
    }
}