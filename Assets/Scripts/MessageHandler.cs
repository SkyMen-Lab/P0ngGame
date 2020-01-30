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
        if (message[0] == '0' || message[0] == '1') return MessageType.Movement;
        if (message == "finish") return MessageType.FinishGame;
        return null;
    }
    
    
        
}