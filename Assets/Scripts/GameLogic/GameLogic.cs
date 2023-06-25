using System;
using System.Collections.Generic;
using System.Linq;
using MarkerClasses;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class GameLogic : MonoBehaviour
    {
        // list of all models displayed by VisualMarkers
        // [SerializeField] private List<GameObject> prefabList;

        // list of all sound-clips played by AudioMarkers
        // [SerializeField] private List<AudioClip> soundClipList;

        // marker id -> marker object
        private Dictionary<int, Marker> _idToMarkerDictionary;
        // maps each id to id of unique matching marker
        private Dictionary<int, int> _matchingMarkerIDs;

        // list of pairs of matching contents (audio-clip and model)
        [SerializeField] private List<MatchingMarkerContents> matchingMarkerContents;
        // list of ids of all visualMarkers (known a priori)
        [SerializeField] private List<int> visualMarkerIDs;
        // list of ids af all audioMarkers (known a priori)
        [SerializeField] private List<int> audioMarkerIDs;
        [Header("Debug")]
        [SerializeField] private bool debug;

        private void Start()
        {
            InitMarkers();
        }

        private void InitMarkers()
        {
            _idToMarkerDictionary = new Dictionary<int, Marker>();
            _matchingMarkerIDs = new Dictionary<int, int>();

            if (!debug) {
                // shuffle ids in order to keep randomized at each init while still iterating linearly
                visualMarkerIDs = visualMarkerIDs.OrderBy(_ => Random.value).ToList();
                audioMarkerIDs = audioMarkerIDs.OrderBy(_ => Random.value).ToList();
            }
            
            // iterate over matchingMarkers, create marker of respective type for each element
            for (var i = 0; i < visualMarkerIDs.Count; i++)
            {
                var match = matchingMarkerContents[i];
                var audioMarkerID = audioMarkerIDs[i];
                var visualMarkerID = visualMarkerIDs[i];
                
                _idToMarkerDictionary.Add(audioMarkerID, new AudioMarker(audioMarkerID, match.AudioClip));
                _idToMarkerDictionary.Add(visualMarkerID, new VisualMarker(visualMarkerID, match.VisualModel));
                _matchingMarkerIDs.Add(audioMarkerID, visualMarkerID);
                _matchingMarkerIDs.Add(visualMarkerID, audioMarkerID);
            }
        }

        public void OnTargetFound()
        {
            Debug.Log("FoundTarget");   
        }
    }
}