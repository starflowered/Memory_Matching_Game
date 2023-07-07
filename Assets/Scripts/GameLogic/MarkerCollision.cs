using UnityEngine;
using Vuforia;

namespace GameLogic
{
    public class MarkerCollision : MonoBehaviour
    {
        private int _id;
        
        private void Start()
        {
            // visual marker has trigger -> it is this object
            // get the ImageTargetBehaviour to access the id of this marker
            if (!transform.TryGetComponent<ImageTargetBehaviour>(out var visualMarker))
            {
                Debug.LogError("This object has no ImageTargetBehaviour component!");
                return;
            }

            // if the marker has no id, throw error
            if (!visualMarker.ID.HasValue)
            {
                Debug.LogError($"The visual marker \"{name}\" has no id!");
                return;
            }

            // save the id to avoid multiple "GetComponent" calls
            _id = visualMarker.ID.Value;
        }

        /// <summary>
        /// Used for detecting collisions between a visual and an audio marker. All audio markers have colliders and
        /// rigidbodies, all visual marker have triggers and the MarkerCollision script attached to them.
        /// </summary>
        /// <param name="other">The audio marker with which the visual marker collided</param>
        private void OnTriggerEnter(Collider other)
        {
            Match(other.gameObject);
        }

        /// <summary>
        /// Gets the ids of both collided markers and then invokes the OnMatched event.
        /// </summary>
        /// <param name="other"></param>
        private void Match(GameObject other)
        {
            // audio marker has a collider -> it is the other game object
            if (!other.transform.TryGetComponent<ImageTargetBehaviour>(out var audioMarker))
            {
                Debug.LogError("The collided object has no ImageTargetBehaviour component!");
                return;
            }

            // if they do not have a value -> error
            if (!audioMarker.ID.HasValue)
            {
                Debug.LogError("The audio marker of the match has no id!");
                return;
            }
            
            // notify subscribers
            GameEvents.instance.OnMatched(_id, audioMarker.ID.Value);
        }
    }
}