using System;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// This class is used as a data container. An audio clip is matched to a model. At the start of the game, these
    /// are matched to image targets.
    /// </summary>
    [Serializable]
    public class MatchingMarkerContents
    {
        #region serialized fields

        [SerializeField] [Tooltip("The audio that is played when a marker was recognized.")] 
        private string audioClipName;
        
        [SerializeField] [Tooltip("The model that is spawned on a marker.")]
        private GameObject visualModel;

        #endregion

        #region fields

        // save the ids of the markers that were assigned to this match
        // this is used later when a match was found -> this match can then be eliminated from a list
        private int _audioId;
        private int _visualId;

        #endregion

        #region properties

        public string AudioClipName => audioClipName;

        public GameObject VisualModel => visualModel;

        public int AudioId
        {
            get => _audioId;
            set => _audioId = value;
        }

        public int VisualId
        {
            get => _visualId;
            set => _visualId = value;
        }

        #endregion
    }
}