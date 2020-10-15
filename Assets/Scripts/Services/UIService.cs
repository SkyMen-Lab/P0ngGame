using System;
using Mirror;
using UnityEngine.UI;
using UnityEngine;

namespace Services
{
    public class UIService : NetworkBehaviour
    {
        #region UI Elements
        
        public Text firstTeamLabel, secondTeamLabel, firstTeamScore, secondTeamScore, status, timer,
            teamOnePlayers, teamTwoPlayers;
        public Button finishGameBtn;
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