using System;
using UnityEngine;

namespace GameLogic
{
    public class GameEvents : MonoBehaviour
    {
        public static GameEvents Instance;

        public Action<int, int> Matched;
        
        public Action<int, float> GameFinished;

        private void OnEnable()
        {
            if (Instance != null) 
                Destroy(gameObject);

            Instance = this;
        }

        public void OnMatched(int markerId1, int markerId2)
        {
            Debug.LogWarning($"Found Match between {markerId1} and {markerId2}!");
            Matched?.Invoke(markerId1, markerId2);
        }

        public void OnGameFinished(int comparisonAmounts, float time)
        {
            GameFinished?.Invoke(comparisonAmounts, time);
        }
    }
}