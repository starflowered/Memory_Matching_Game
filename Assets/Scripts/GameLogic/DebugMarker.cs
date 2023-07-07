using UI;
using UnityEngine;
using Vuforia;

namespace GameLogic
{
    public class DebugMarker : MonoBehaviour
    {
        // the menu that should be opened as soon as the debug marker is recognized
        private PauseMenu _menu;
        private ObserverBehaviour _observerBehaviour;
        
        private void Start()
        {
            // get the menu on start up
            _menu = FindObjectOfType<PauseMenu>();
            
            // if the script is not on a vuforia image target, error
            if (!TryGetComponent(out _observerBehaviour))
            {
                Debug.LogError("ImageTargetBehaviour not found!");
                return;
            }
            
            // subscribe to the OnTargetStatusChanged to get notified if this marker was recognized
            _observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        private void OnDisable()
        {
            // unsubscribe from event
            _observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
        }

        /// <summary>
        /// Pauses and unpauses a game, depending on if the debug marker was recognized.
        /// </summary>
        /// <param name="observerBehaviour">The ObserverBehaviour of the image target</param>
        /// <param name="targetStatus">The current status of the image target</param>
        private void OnTargetStatusChanged(ObserverBehaviour observerBehaviour, TargetStatus targetStatus)
        {
            switch (targetStatus.Status)
            {
                default:
                case Status.EXTENDED_TRACKED:
                case Status.NO_POSE:
                    _menu.UnpauseGame();
                    break;
                case Status.LIMITED:
                case Status.TRACKED:
                    _menu.PauseGame();
                    break;
            }
        }
    }
}