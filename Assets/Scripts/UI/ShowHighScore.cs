using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShowHighScore : MonoBehaviour
    {
        [SerializeField] private TMP_Text highScoreStats;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (PlayerPrefs.GetInt("resetHighScore", 0) == 0)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetInt("resetHighScore", 1);
            }
#endif
            var currentComparisons = PlayerPrefs.GetInt("comparisons", 0);
            var currentTime = PlayerPrefs.GetInt("time", 0);

            var time = TimeSpan.FromSeconds(currentTime);

            highScoreStats.SetText(
                $"High score:\nTime: {(currentTime == 0 ? "-" : $"{time.Minutes:00}:{time.Seconds:00}")} min\nComparisons: {(currentComparisons == 0 ? "-" : currentComparisons)}");
        }

        
#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            PlayerPrefs.SetInt("resetHighScore", 0);
        }
#endif

    }
}