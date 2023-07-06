using UnityEngine;
using Vuforia;

namespace GameLogic
{
    public class MarkerCollision : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Match(other.gameObject);
        }

        private void Match(GameObject other)
        {
            // visual marker has trigger -> it is this object
            if (!transform.TryGetComponent<ImageTargetBehaviour>(out var visualMarker))
            {
                Debug.LogError("This object has no ImageTargetBehaviour component!");
                return;
            }
            
            // audio marker has a collider -> it is the other game object
            if (!other.transform.TryGetComponent<ImageTargetBehaviour>(out var audioMarker))
            {
                Debug.LogError("The collided object has no ImageTargetBehaviour component!");
                return;
            }

            // if they do not have a value -> error
            if (!visualMarker.ID.HasValue || !audioMarker.ID.HasValue)
            {
                Debug.LogError("At least one marker of the match has no id!");
                return;
            }
            
            // notify subscribers
            GameEvents.Instance.OnMatched(visualMarker.ID.Value, audioMarker.ID.Value);
        }
    }
}