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

    public static MessageType? ProcessPacket(Packet packet)
    {
        if (packet.MetaData == Meta.Connect) return MessageType.InitTeams;
        if (packet.Message == "start") return MessageType.StartGame;
        if (char.IsDigit(packet.Message[0]) || packet.Message[0] == '-') return MessageType.Movement;
        if (packet.Message == "finish") return MessageType.FinishGame;
        return null;
    }

    public static KeyValuePair<string, float> ParseMovement(string message)
    {
        var arr = message.Split(' ');
        var mov = float.Parse(arr[0]);
        var code = arr[1];
        
        return new KeyValuePair<string, float>(code, mov);
    }
}