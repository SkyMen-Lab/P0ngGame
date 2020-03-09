using System;
using System.Collections.Generic;
using Models;
using Newtonsoft.Json;

public static class MessageHandler
{

    public static KeyValuePair<string, float> ParseMovement(string message)
    {
        var arr = message.Split(' ');
        var mov = float.Parse(arr[0]);
        var code = arr[1];
        
        return new KeyValuePair<string, float>(code, mov);
    }
}