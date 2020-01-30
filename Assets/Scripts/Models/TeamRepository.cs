using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Models
{
    public class TeamRepository
    {
        private static List<Team> _teams;
        
        private TeamRepository() { }

        public static readonly object _lock = new object();

        public static List<Team> GetTeams()
        {
            if (_teams == null)
            {
                lock (_lock)
                {
                    if (_teams == null)
                    {
                        _teams = new List<Team>();
                    }
                }
            }

            return _teams;
        }

        public Team FindTeam(Predicate<Team> expression)
        {
            return _teams.Find(expression);
        }

        public bool UpdateTeam(Team team)
        {
            var currentTeam = FindTeamByCode(team.Code);
            if (currentTeam == null)
                return false;
            _teams.Remove(currentTeam);
            _teams.Add(team);
            return true;
        }

        public bool Init(List<Team> teams)
        {
            if (_teams.Count == 2) return false;
            _teams.AddRange(teams);
            return true;
        }

        public Team FindTeamByCode(string code)
        {
            return FindTeam(x => string.Equals(code, x.Code));
        }
    }
}