using System;
using System.Collections.Generic;
using Models;
using Newtonsoft.Json;

public static class MessageHandler
{
    public enum MessageType
    {
        InitTeams,
        StartGame,
        Movement,
        FinishGame
    }

    public static MessageType? ProcessMessageType(string message)
    {
        if (string.IsNullOrEmpty(message))
            return null;
        
        if (message[0] == '{') return MessageType.InitTeams;
        if (message == "start") return MessageType.StartGame;
        if (char.IsDigit(message[0])) return MessageType.Movement;
        if (message == "finish") return MessageType.FinishGame;
        return null;
    }

    public static List<Team> ParseTeams(string message)
    {
        if (string.IsNullOrEmpty(message))
            return null;

        var list = JsonConvert.DeserializeObject<List<Team>>(message);
        return list;
    }

    public static KeyValuePair<string, float>? ParseMovement(string message)
    {
        if (string.IsNullOrEmpty(message))
            return null;
        var arr = message.Split(' ');
        var mov = float.Parse(arr[0]);
        var code = arr[1];
        
        return new KeyValuePair<string, float>(code, mov);
    }
}