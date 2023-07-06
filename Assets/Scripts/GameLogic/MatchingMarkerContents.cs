using System;
using UnityEngine;

namespace GameLogic
{
    [Serializable]
    public class MatchingMarkerContents
    {
        [SerializeField] private string audioClipName;
        [SerializeField] private GameObject visualModel;

        private int _audioId;
        private int _visualId;
        
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
    }
}