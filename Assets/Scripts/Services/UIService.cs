using System;
using UnityEngine.UI;
using UnityEngine;

namespace Services
{
    public class UIService : MonoBehaviour
    {
        #region UI Elements

        public Text firstTeamLabel, secondTeamLabel, firstTeamScore, secondTeamScore, status, timer,
            teamOnePlayers, teamTwoPlayers;
        public static UIService Instance;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}