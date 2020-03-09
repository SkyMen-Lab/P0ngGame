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
        InitTeams,
        UpdateNumberOfPlayers,
        Movement,
        Score,
        FinishGame
    }
}