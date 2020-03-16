using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Models
{
    public class TeamRepository
    {
        private List<Team> _teams;
        private static TeamRepository _instance;
        
        private TeamRepository() { }

        public static readonly object _lock = new object();

        public static TeamRepository GetTeamRepository()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TeamRepository();
                        _instance._teams = new List<Team>(2);
                    }
                }
            }

            return _instance;
        }

        public Team FindTeam(Predicate<Team> expression)
        {
            return _teams.Find(expression);
        }

        public List<Team> GetList()
        {
            return _teams;
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

        public void IncrementScore(Side teamSide)
        {
            if (teamSide == Side.Left) _teams[0].Score++;
            else _teams[1].Score++;
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