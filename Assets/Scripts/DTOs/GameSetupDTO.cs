using System.Collections.Generic;
using Models;

namespace DTOs
{
    public class GameSetupDTO
    {
        public string Code { get; set; }
        public int Duration { get; set; }
        public List<Team> Teams { get; set; }
    }
}