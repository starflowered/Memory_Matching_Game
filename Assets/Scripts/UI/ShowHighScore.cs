using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ShowHighScore : MonoBehaviour
    {
        #region serialized fields

        [SerializeField] [Tooltip("The label in the main menu scene where the high score stats are shown.")]
        private TMP_Text highScoreStats;

        #endregion

        private void OnEnable()
        {
#if UNITY_EDITOR
            // reset the high score with every new start of the game
            // is only done when using the unity editor to test the high score feature
            if (PlayerPrefs.GetInt("resetHighScore", 0) == 0)
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.SetInt("resetHighScore", 1);
            }
#endif
            // get the current high score amount of comparisons and time
            var currentComparisons = PlayerPrefs.GetInt("comparisons", 0);
            var currentTime = PlayerPrefs.GetInt("time", 0);

            // convert the amount of seconds to a time span object (easy access to minutes, hours, ...)
            var time = TimeSpan.FromSeconds(currentTime);

            // set the current high score to the label
            highScoreStats.SetText($"High score:\nTime: {(currentTime == 0 ? "-" : $"{time.Minutes:00}:{time.Seconds:00}")} min\nComparisons: {(currentComparisons == 0 ? "-" : currentComparisons)}");
        }

        
#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            // set the reset label if the player prefs should be cleared before starting the application next time
            // only done if for debugging in the unity editor
            PlayerPrefs.SetInt("resetHighScore", 0);
        }
#endif

    }
}