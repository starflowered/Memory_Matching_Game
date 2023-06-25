using System;
using MarkerClasses;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameLogic
{
    [Serializable]
    public class MatchingMarkerContents
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private GameObject visualModel;

        public AudioClip AudioClip => audioClip;

        public GameObject VisualModel => visualModel;
    }
}