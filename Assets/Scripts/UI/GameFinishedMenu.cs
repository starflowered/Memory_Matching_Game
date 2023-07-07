using System;
using GameLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameFinishedMenu : MonoBehaviour
    {
        #region serialized fields 

        [SerializeField] [Tooltip("The menu that pops up when all matches were found.")] 
        private GameObject gameFinishedMenu;
        
        [SerializeField] [Tooltip("The label which shows the stats (should be on the Game-Finished-Menu).")] 
        private TMP_Text stats;

        [SerializeField] [Tooltip("If a new high score was reached, it is shown in this label.")] 
        private TMP_Text newHighScoreLabel;

        #endregion

        private void Start()
        {
            // subscribe to the gameFinished event
            GameEvents.instance.gameFinished += GameFinished;
        }

        private void OnDisable()
        {
            // unsubscribe from the gameFinished event
            GameEvents.instance.gameFinished -= GameFinished;
        }

        /// <summary>
        /// Show the Game-Finished menu and fill in the statistics
        /// </summary>
        /// <param name="comparisons"></param>
        /// <param name="time"></param>
        private void GameFinished(int comparisons, TimeSpan time)
        {
            // show menu
            gameFinishedMenu.SetActive(true);

            // set high score if this run was better
            SetHighScore(comparisons, time);

            // set the statistics
            stats.SetText($"Stats:\n\tTime: {time.Minutes:00}:{time.Seconds:00} min\n\tComparisons: {comparisons}");
        }

        /// <summary>
        /// Look up the current high score and overwrite it, if the given score is higher.
        /// </summary>
        /// <param name="comparisons">The amount of comparisons needed to find all matches</param>
        /// <param name="time">The time the players needed to find all matches</param>
        private void SetHighScore(int comparisons, TimeSpan time)
        {
            // get the current score from the player preferences
            var currentComparisons = PlayerPrefs.GetInt("comparisons", -1);
            var currentTime = PlayerPrefs.GetInt("time", -1);

            var newHighScoreReached = false;
            
            // check if the amount of comparisons is less than the last set comparison high score
            if (currentComparisons == -1 || currentComparisons > comparisons)
            {
                PlayerPrefs.SetInt("comparisons", comparisons);
                newHighScoreReached = true;
            }

            // check if the time taken is less than the last set time high score
            if (currentTime < 0 || currentTime > time.TotalSeconds)
            {
                PlayerPrefs.SetInt("time", time.Seconds);
                newHighScoreReached = true;
            }

            // show the high score label if a new high score was reached
            ShowNewHighScoreLabel(newHighScoreReached);
            
            // notify subscriber if a new high score was reached
            if (newHighScoreReached) 
                GameEvents.instance.OnNewHighScoreReached();
        }

        /// <summary>
        /// Show the NewHighScore label depending on the given value 'show'.
        /// </summary>
        /// <param name="show">If true, show that the players have reached a new high score.
        /// If false, clear the label</param>
        private void ShowNewHighScoreLabel(bool show)
        {
            newHighScoreLabel.SetText(show? "!! NEW HIGH SCORE !!" : "");
        }
    }
}