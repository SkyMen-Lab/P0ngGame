using System.Collections.Generic;
using Models;

namespace DTOs
{
    public class GameFinishedDTO
    {
        public string GameCode { get; set; }
        public string WinnerCode { get; set; }
        public int MaxSpeedLevel { get; set; }
        public List<Team> Teams { get; set; }
    }
}