using System.Collections.Generic;
using System.Linq;
using MarkerClasses;
using Unity.VisualScripting;
using UnityEngine;
using Vuforia;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class GameLogic : MonoBehaviour
    {
        #region Serialized fields

        [Header("Marker")] 
        [SerializeField] private List<ImageTargetBehaviour> visualMarkers;
        [SerializeField] private List<ImageTargetBehaviour> audioMarkers;
        
        [Header("Memory Matches")]
        // list of pairs of matching contents (audio-clip and model)
        [SerializeField] private List<MatchingMarkerContents> matchingMarkerContents;

        [Header("Debug")]
        [SerializeField] private bool debug;

        #endregion
        
        #region fields

        private AudioMarker _currentAudioMarker;
        private VisualMarker _currentVisualMarker;

        // marker id -> marker object
        private Dictionary<int, Marker> _idToMarkerDictionary;
        // maps each id to id of unique matching marker
        private Dictionary<int, int> _matchingMarkerIDs;

        private AudioSource _audioSource;

        #endregion
        
        
        private void Start()
        {
            InitMarkerIds();
            
            SpawnVisualMarkerModels();
        }

        private void InitMarkerIds()
        {
            _idToMarkerDictionary = new Dictionary<int, Marker>();
            _matchingMarkerIDs = new Dictionary<int, int>();
            
            for (var i = 0; i < visualMarkers.Count; i++)
            {
                // get visual marker from list
                var visualMarker = visualMarkers[i];
                
                // shuffle audio marker list and get random one
                if (!debug) audioMarkers = audioMarkers.OrderBy(_ => Random.value).ToList();
                var audioMarker = audioMarkers[i];
                
                // if they have no id, error
                if (!visualMarker.ID.HasValue || !audioMarker.ID.HasValue)
                {
                    Debug.LogError($"Marker {visualMarker.name} has no ID!");
                    continue;
                }
                
                // get a random marker match
                if (!debug) matchingMarkerContents = matchingMarkerContents.OrderBy(_ => Random.value).ToList();
                var match = matchingMarkerContents[i];
                
                // add visual and audio marker to the (id -> marker) dictionary
                _idToMarkerDictionary.Add(audioMarker.ID.Value, new AudioMarker(audioMarker.ID.Value, audioMarker, match.AudioClip));
                _idToMarkerDictionary.Add(visualMarker.ID.Value, new VisualMarker(visualMarker.ID.Value, visualMarker, match.VisualModel));
                
                // make sure that visual and audio marker are connected
                _matchingMarkerIDs.Add(visualMarker.ID.Value, audioMarker.ID.Value);
                _matchingMarkerIDs.Add(audioMarker.ID.Value, visualMarker.ID.Value);
            }
        }
        
        private void SpawnVisualMarkerModels()
        {
            // place game object on top of marker
        }

        public void OnTargetFound(ImageTargetBehaviour marker)
        {
            // Debug.Log("FoundTarget");
            // this rounds markers already initialized
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }
            
            var id = marker.ID.Value;
            if (_idToMarkerDictionary[id].GetType() == typeof(AudioMarker))
            {
                if (_currentAudioMarker.IsUnityNull())
                {
                    _currentAudioMarker = (AudioMarker)_idToMarkerDictionary[id];
                    PlayMarkerSound();
                }
                else return;
            }
            else
            {
                if (_currentVisualMarker.IsUnityNull())
                {
                    _currentVisualMarker = (VisualMarker)_idToMarkerDictionary[id];
                    DisplayMarkerModel();
                }
                else return;
            }
            
            
        }

        private void DisplayMarkerModel()
        {
            // TODO
        }

        private void PlayMarkerSound()
        {
            _audioSource.clip = _currentAudioMarker.SoundClip;
            _audioSource.PlayDelayed(1);
        }
    }
}