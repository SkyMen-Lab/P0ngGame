using System;
using System.Collections;
using Services;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : NetworkBase
{
    public Button button;
    private void Start()
    {
        OnMessageReceivedEvent += delegate(string message) {
            Debug.unityLogger.Log(message);
        };
        
        button.onClick.AddListener(TestMSG);
    }

    private void TestMSG()
    {
        SendMessageToServer("Hello");
    }
}