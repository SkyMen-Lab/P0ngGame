using Newtonsoft.Json;

namespace Models
{
    public class Message
    {
        public GameAction ContentType { get; set; }
        public string Content { get; set; }

        public Message(GameAction action, string content)
        {
            ContentType = action;
            Content = content;
        }
        
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Message FromJson(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }
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