using System;
using GameLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameFinishedMenu : MonoBehaviour
    {
        [SerializeField] [Tooltip("")] private GameObject gameFinishedMenu;
        [SerializeField] [Tooltip("")] private TMP_Text stats;
        [SerializeField] [Tooltip("")] private TMP_Text newHighScoreLabel;

        private void Start()
        {
            GameEvents.instance.gameFinished += GameFinished;
        }

        private void GameFinished(int comparisons, TimeSpan time)
        {
            gameFinishedMenu.SetActive(true);

            SetHighScore(comparisons, time);

            stats.SetText($"Stats:\n\tTime: {time.Minutes:00}:{time.Seconds:00} min\n\tComparisons: {comparisons}");
        }

        private void SetHighScore(int comparisons, TimeSpan time)
        {
            var currentComparisons = PlayerPrefs.GetInt("comparisons", -1);
            var currentTime = PlayerPrefs.GetInt("time", -1);

            var newHighScoreSet = false;
            
            if (currentComparisons == -1 || currentComparisons > comparisons)
            {
                PlayerPrefs.SetInt("comparisons", comparisons);
                GameEvents.instance.OnNewHighScoreReached();
                newHighScoreSet = true;
            }

            if (currentTime < 0 || currentTime > time.TotalSeconds)
            {
                PlayerPrefs.SetInt("time", time.Seconds);
                GameEvents.instance.OnNewHighScoreReached();
                newHighScoreSet = true;
            }

            ShowNewHighScoreLabel(newHighScoreSet);
        }

        private void ShowNewHighScoreLabel(bool show)
        {
            newHighScoreLabel.SetText(show? "!! NEW HIGH SCORE !!" : "");
        }
    }
}