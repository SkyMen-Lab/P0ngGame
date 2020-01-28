using System;
using Services;
using UnityEngine;

namespace DefaultNamespace
{
    public class NetworkManager : NetworkBase
    {
        private void Start()
        {
            ConnectToServerApi("127.0.0.1", 5050);
            OnMessageReceivedEvent += delegate(string message) {
                Debug.unityLogger.Log(message);
            };
        }
    }
}