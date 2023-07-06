using UI;
using UnityEngine;
using Vuforia;

namespace GameLogic
{
    public class DebugMarker : MonoBehaviour
    {
        private PauseMenu _menu;
        
        private void Start()
        {
            _menu = FindObjectOfType<PauseMenu>();
            
            if (!TryGetComponent<ObserverBehaviour>(out var imageTargetBehaviour))
            {
                Debug.Log("ImageTargetBehaviour not found!");
                return;
            }
            
            imageTargetBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }

        private void OnTargetStatusChanged(ObserverBehaviour arg1, TargetStatus arg2)
        {
            switch (arg2.Status)
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