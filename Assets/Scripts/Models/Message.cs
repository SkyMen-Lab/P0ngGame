namespace Models
{
    public class Message
    {
        public GameAction ContentType { get; set; }
        public string Content { get; set; }
    }
    
    public enum GameAction
    {
        StartGame,
        InitGame,
        UpdateNumberOfPlayers,
        Movement,
        Score,
        FinishGame
    }
}