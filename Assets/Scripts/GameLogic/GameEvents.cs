using System;
using UnityEngine;

namespace GameLogic
{
    public class GameEvents : MonoBehaviour
    {
        // singleton
        public static GameEvents instance;

        // events
        public Action<int, int> matched;
        public Action<int, TimeSpan> gameFinished;
        public Action newHighScoreReached;

        private void OnEnable()
        {
            // create singleton
            if (instance != null) 
                Destroy(gameObject);

            instance = this;
        }
        
        #region invoke event methods

        public void OnMatched(int markerId1, int markerId2)
        {
            matched?.Invoke(markerId1, markerId2);
        }

        public void OnGameFinished(int comparisonAmounts, TimeSpan time)
        {
            gameFinished?.Invoke(comparisonAmounts, time);
        }

        public void OnNewHighScoreReached()
        {
            newHighScoreReached?.Invoke();
        }

        #endregion
    }
}