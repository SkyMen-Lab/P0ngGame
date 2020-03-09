using System;
using Newtonsoft.Json;

namespace Models
{
    public class Team
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public float PuddleSpeed { get; set; }
        public int NumberOfPlayers { get; set; }
        
        [JsonIgnore]
        public Side Side { get; set; }
    }

    public enum Side
    {
        Left,
        Right
    }
}