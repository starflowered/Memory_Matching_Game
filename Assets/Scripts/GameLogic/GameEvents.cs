using System;
using UnityEngine;

namespace GameLogic
{
    public class GameEvents : MonoBehaviour
    {
        public static GameEvents instance;

        public Action<int, int> matched;
        
        public Action<int, TimeSpan> gameFinished;
        
        public Action newHighScoreReached;

        private void OnEnable()
        {
            if (instance != null) 
                Destroy(gameObject);

            instance = this;
        }

        public void OnMatched(int markerId1, int markerId2)
        {
            Debug.LogWarning($"Found Match between {markerId1} and {markerId2}!");
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
    }
}