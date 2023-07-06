using GameLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameFinishedMenu : MonoBehaviour
    {
        [SerializeField] [Tooltip("")]
        private GameObject gameFinishedMenu;
        
        [SerializeField] [Tooltip("")]
        private TMP_Text stats;
        
        private void Start()
        {
            GameEvents.Instance.GameFinished += GameFinished;
        }

        private void GameFinished(int comparisons, float time)
        {
            gameFinishedMenu.SetActive(true);
            
            stats.SetText($"Stats:\n\tTime: {time} seconds\n\tComparisons: {comparisons}");
        }
    }
}