using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using MarkerClasses;
using Unity.VisualScripting;
using UnityEngine;
using Vuforia;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class GameLogic : MonoBehaviour
    {
        #region constants

        // path to the vuforia database where all markers are saved
        private const string DatabasePath = "Vuforia/Colored_Markers_Mirrored.xml";
        
        // the rotation speed of the object spawned on top of a marker
        private const float RotationSpeed = 40f;

        #endregion

        #region Serialized fields

        [Header("Memory Matches")]
        [SerializeField] [Tooltip("List of pairs of matching contents (audio-clip and model)")]
        private List<MatchingMarkerContents> matchingMarkerContents;

        [SerializeField] [Tooltip("Prefab for showing if an evaluation resulted in a/no match")]
        private GameObject matchEvaluationPrefab;
        
        [SerializeField] [Tooltip("Material for a valid match")]
        private Material validMatchMaterial;
        
        [SerializeField] [Tooltip("Material for an invalid ")]
        private Material invalidMatchMaterial;
        
        [SerializeField] [Tooltip("Material to show that players should remove their cards from the play field")]
        private Material removeCardsFromPlayFieldMaterial;
        
        [SerializeField] [Tooltip("Material to show that the visual player should return their card to their side of the mat")]
        private Material returnCardToVisualPlayerMaterial;
        
        [SerializeField] [Tooltip("Material to show that the audio player should return their card to their side of the mat")]
        private Material returnCardToAudioPlayerMaterial;
        
        [Header("Debug")] 
        [SerializeField] [Tooltip("If the run is a debug run, tick this")]
        private bool debug;

        #endregion

        #region fields

        // the audio manager is used to play sound
        private AudioManager _audioManager;

        // save the currently found markers
        private AudioMarker _currentAudioMarker;
        private VisualMarker _currentVisualMarker;

        // used for evaluation purposes, each marker get one icon that can be show above it
        private Transform _audioMarkerEvaluationIcon;
        private Transform _visualMarkerEvaluationIcon;
        // save the renderers to change their materials, based on the current state
        private Renderer _audioMarkerEvaluationIconRenderer;
        private Renderer _visualMarkerRenderer;
        // used to signal that a collision was detected
        private bool _collisionDetected;

        // save the markers' ImageTargetBehaviour components to save computation time 
        private List<ImageTargetBehaviour> _visualImageTargetBehaviours;
        private List<ImageTargetBehaviour> _audioImageTargetBehaviours;

        // save all ImageTargetBehaviours
        private Dictionary<int, ImageTargetBehaviour> _imageTargetBehaviours;
        // save colliders of markers for better control of them
        private Dictionary<int, Collider> _colliders;
        // save instantiated game objects for better control of them
        private Dictionary<int, GameObject> _instances;

        // marker id -> marker object
        private Dictionary<int, Marker> _idToMarkerDictionary;

        // maps each id to id of unique matching marker
        private Dictionary<int, int> _matchingMarkerIDs;

        // if a match was found, it is removed from this list. If the list is empty the game is finished
        private List<MatchingMarkerContents> _matchingMarkerContentsCopy;
        // the amount of comparisons that is needed to find all matches
        private int _amountOfComparisons;
        // the time when the game was started
        private DateTime _startTime;
        // the time of when the game was finished
        private DateTime _endTime;
        // the overall time that took the players to find all matches
        private int _timeTookToFindAllMatches;
        
        #endregion

        #region unity event methods

        private void OnEnable()
        {
            // subscribe to events
            VuforiaApplication.Instance.OnVuforiaStarted += InitAll;
            GameEvents.instance.matched += StartMatchEvaluation;
        }

        private void Start()
        {
#if !UNITY_EDITOR
            // set debug mode to false if not in unity editor
            debug = false;
#endif

            // get the audio manager
            _audioManager = FindObjectOfType<AudioManager>();

            // instantiate the evaluation icons and disable them
            _audioMarkerEvaluationIcon = Instantiate(matchEvaluationPrefab).transform;
            _audioMarkerEvaluationIcon.gameObject.SetActive(false);
            if (!_audioMarkerEvaluationIcon.transform.GetChild(0).TryGetComponent(out _audioMarkerEvaluationIconRenderer))
                Debug.LogError($"Renderer was not found on game object {_audioMarkerEvaluationIcon.name}");

            _visualMarkerEvaluationIcon = Instantiate(matchEvaluationPrefab).transform;
            _visualMarkerEvaluationIcon.gameObject.SetActive(false);
            if (!_visualMarkerEvaluationIcon.transform.GetChild(0).TryGetComponent(out _visualMarkerRenderer))
                Debug.LogError($"Renderer was not found on game object {_visualMarkerEvaluationIcon.name}");
            
            // set the start time for time measurement
            _startTime = DateTime.Now;
        }

        private void OnDisable()
        {
            // unsubscribe from all events
            
            VuforiaApplication.Instance.OnVuforiaStarted -= InitAll;
            GameEvents.instance.matched -= StartMatchEvaluation;
            
            foreach (var imageTargetBehaviour in _audioImageTargetBehaviours)
            {
                imageTargetBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
            }

            foreach (var imageTargetBehaviour in _visualImageTargetBehaviours)
            {
                imageTargetBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
            }
        }

        #endregion

        /// <summary>
        /// Spawn models above markers in the scene and assign matching markers.
        /// </summary>
        private void InitAll()
        {
            SpawnVisualMarkerModels();

            InitMarkerIds();
        }

        /// <summary>
        /// Spawn models above all visual markers.
        /// </summary>
        private void SpawnVisualMarkerModels()
        {
            // init dictionaries and lists
            _imageTargetBehaviours = new Dictionary<int, ImageTargetBehaviour>();
            _colliders = new Dictionary<int, Collider>();
            _visualImageTargetBehaviours = new List<ImageTargetBehaviour>();
            _audioImageTargetBehaviours = new List<ImageTargetBehaviour>();
            
            // create image targets for each marker inside the database
            for (var i = 1; i <= matchingMarkerContents.Count; i++) 
                CreateInstantImageTarget($"visualMarker0{i}", false);

            for (var i = 1; i <= matchingMarkerContents.Count; i++) 
                CreateInstantImageTarget($"audioMarker0{i}", true);

            CreateDebugImageTarget();
        }

        /// <summary>
        /// Assign the given match details to markers.
        /// </summary>
        private void InitMarkerIds()
        {
            // init dictionaries
            _idToMarkerDictionary = new Dictionary<int, Marker>();
            _matchingMarkerIDs = new Dictionary<int, int>();
            _instances = new Dictionary<int, GameObject>();

            // assign sounds and objects to markers
            for (var i = 0; i < _visualImageTargetBehaviours.Count; i++)
            {
                // get visual marker from list
                var visualMarker = _visualImageTargetBehaviours[i];

                // shuffle audio marker list and get random one
                if (!debug) _audioImageTargetBehaviours = _audioImageTargetBehaviours.OrderBy(_ => Random.value).ToList();
                var audioMarker = _audioImageTargetBehaviours[i];

                // check ids
                if (!visualMarker.ID.HasValue)
                {
                    Debug.LogError($"Marker {visualMarker.name} has no ID!");
                    continue;
                }

                if (!audioMarker.ID.HasValue)
                {
                    Debug.LogError($"Marker {audioMarker.name} has no ID!");
                    continue;
                }

                // get a random marker match
                if (!debug) matchingMarkerContents = matchingMarkerContents.OrderBy(_ => Random.value).ToList();
                var match = matchingMarkerContents[i];

                // add marker ids to match
                match.AudioId = audioMarker.ID.Value;
                match.VisualId = visualMarker.ID.Value;
                
                // instantiate marker object
                var animal = Instantiate(match.VisualModel, Vector3.zero, Quaternion.identity);
                animal.transform.parent = visualMarker.gameObject.transform;
                animal.transform.Rotate(new Vector3(-30, 0, 30), Space.World);
                animal.transform.position = Vector3.up * 0.5f;

                // and add it to the _instances dictionary
                _instances.Add(visualMarker.ID.Value, animal);

                // add collision detection to each marker
                // visual marker gets a trigger with the MarkerCollision script
                // --> if a collider enters the trigger box, the script handles this
                var trigger = visualMarker.gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = Vector3.up * 0.5f;
                trigger.size = new Vector3(2, 1, 2);
                trigger.enabled = false;
                _colliders.Add(visualMarker.ID.Value, trigger);

                visualMarker.gameObject.AddComponent<MarkerCollision>();

                // the audio marker has a collider 
                // --> only if a visual and an audio marker intersect, a potential match is evaluated
                var col = audioMarker.gameObject.AddComponent<BoxCollider>();
                col.center = Vector3.up * 0.5f;
                col.size = new Vector3(2, 1, 2);
                col.enabled = false;
                _colliders.Add(audioMarker.ID.Value, col);

                // the object needs a rigidbody to be able to enter a trigger
                var rigidBody = audioMarker.gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;

                // add visual and audio marker to the (id -> marker) dictionary
                _idToMarkerDictionary.Add(audioMarker.ID.Value,
                    new AudioMarker(audioMarker.ID.Value, audioMarker, match.AudioClipName));
                _idToMarkerDictionary.Add(visualMarker.ID.Value,
                    new VisualMarker(visualMarker.ID.Value, visualMarker, match.VisualModel));

                // make sure that visual and audio marker are connected
                _matchingMarkerIDs.Add(visualMarker.ID.Value, audioMarker.ID.Value);
                _matchingMarkerIDs.Add(audioMarker.ID.Value, visualMarker.ID.Value);
            }
            
            // make a copy of the match information, this list can be modified
            _matchingMarkerContentsCopy = new List<MatchingMarkerContents>(matchingMarkerContents);
        }

        /// <summary>
        /// Create a target for a given marker.
        /// </summary>
        /// <param name="markerName">The name of the marker inside of the database</param>
        /// <param name="isAudioMarker">If true, the target is treated as an audio marker, if false, as a visual marker</param>
        private void CreateInstantImageTarget(string markerName, bool isAudioMarker)
        {
            // Debug.Log($"Adding target {markerName}");
            
            // create image target
            var imageTargetBehaviour = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(DatabasePath, markerName);
            var defaultObserverEventHandler = imageTargetBehaviour.gameObject.AddComponent<DefaultObserverEventHandler>();
            defaultObserverEventHandler.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked;

            // add subscriber
            imageTargetBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

            // check if the image target has an id
            if (!imageTargetBehaviour.ID.HasValue)
            {
                Debug.LogError($"{imageTargetBehaviour.name} does not have an ID!");
                return;
            }

            // add the ImageTargetBehaviour to a dictionary (int -> ImageTargetBehaviour)
            _imageTargetBehaviours.Add(imageTargetBehaviour.ID.Value, imageTargetBehaviour);

            // add to lists
            if (isAudioMarker)
                _audioImageTargetBehaviours.Add(imageTargetBehaviour);
            else
                _visualImageTargetBehaviours.Add(imageTargetBehaviour);
        }

        /// <summary>
        /// Create the debug image target.
        /// </summary>
        private void CreateDebugImageTarget()
        {
            // create image target
            var imageTargetBehaviour =
                VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(DatabasePath, "debugMarker");
            var defaultObserverEventHandler =
                imageTargetBehaviour.gameObject.AddComponent<DefaultObserverEventHandler>();
            defaultObserverEventHandler.StatusFilter = DefaultObserverEventHandler.TrackingStatusFilter.Tracked;

            // add debug marker script
            imageTargetBehaviour.gameObject.AddComponent<DebugMarker>();
        }

        /// <summary>
        /// Method that is called if the status of a marker has changed.
        /// </summary>
        /// <param name="marker">The marker which status has changed</param>
        /// <param name="status">The current status</param>
        private void OnTargetStatusChanged(ObserverBehaviour marker, TargetStatus status)
        {
            // check if the marker has an ID
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }

            switch (status.Status)
            {
                default:
                case Status.EXTENDED_TRACKED:
                case Status.NO_POSE:
                    TargetLost(_imageTargetBehaviours[marker.ID.Value]);
                    break;

                case Status.LIMITED:
                case Status.TRACKED:
                    TargetFound(_imageTargetBehaviours[marker.ID.Value]);
                    break;
            }
        }

        /// <summary>
        /// A new target has been found by the Vuforia camera. 
        /// </summary>
        /// <param name="marker">The marker that was recognized</param>
        private void TargetFound(ImageTargetBehaviour marker)
        {
            // check if the marker has an ID
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }

            var id = marker.ID.Value;

            // enable the collider of the marker so it can collide with other markers
            _colliders[id].enabled = true;

            // if the found marker is an audio marker
            if (_idToMarkerDictionary[id].GetType() == typeof(AudioMarker))
            {
                // if the marker was already assigned, do not override
                // -> will not recognize two audio markers
                if (!_currentAudioMarker.IsUnityNull())
                    return;

                // set recognized audio marker
                _currentAudioMarker = (AudioMarker)_idToMarkerDictionary[id];

                // Debug.Log("Found Audio marker");

                if (_collisionDetected)
                {
                    _audioMarkerEvaluationIcon.gameObject.SetActive(false);
                    return;
                }

                // play corresponding sound
                PlayMarkerSound();
            }
            else
            {
                // analogue to the audio marker part (see above)
                if (!_currentVisualMarker.IsUnityNull())
                {
                    _instances[_currentVisualMarker.Id].SetActive(true);
                    return;
                }

                Debug.Log("Found Visual marker");

                // set recognized visual marker
                _currentVisualMarker = (VisualMarker)_idToMarkerDictionary[id];

                if (_collisionDetected)
                {
                    _visualMarkerEvaluationIcon.gameObject.SetActive(false);
                    return;
                }

                // display marker
                DisplayMarkerModel(id);
            }
        }

        /// <summary>
        /// Display the model of the given marker id.
        /// </summary>
        /// <param name="id">The id of the marker of which the spawned model should be displayed</param>
        private void DisplayMarkerModel(int id)
        {
            // enable spawned game object
            _instances[id].SetActive(true);

            // start rotating the object
            StartCoroutine(RotateModel(_instances[id], RotationSpeed));
        }

        /// <summary>
        /// Rotate a given object with a given speed.
        /// </summary>
        /// <param name="instance">The object that should be rotated</param>
        /// <param name="rotationSpeed">The speed with which the object should be rotated</param>
        private static IEnumerator RotateModel(GameObject instance, float rotationSpeed)
        {
            // rotate around self 
            while (instance.activeSelf)
            {
                instance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }

        /// <summary>
        /// PLay the sound of the recognized marker.
        /// </summary>
        private void PlayMarkerSound()
        {
            _audioManager.Play(_currentAudioMarker.SoundClipName);
        }

        /// <summary>
        /// A target has been lost by the Vuforia camera. 
        /// </summary>
        /// <param name="marker">The marker that was lost</param>
        private void TargetLost(ImageTargetBehaviour marker)
        {
            // check if the marker has an ID
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }

            var id = marker.ID.Value;

            // disable the collider so that no collision happens accidentally
            _colliders[id].enabled = false;

            // if the lost marker was an audio marker
            if (_idToMarkerDictionary[id].GetType() == typeof(AudioMarker))
            {
                // remove the current marker and disable the corresponding evaluation icon
                _currentAudioMarker = null;
                _audioMarkerEvaluationIcon.gameObject.SetActive(false);
            }

            else
            {
                // hide the spawned object of the marker
                _instances[id].SetActive(false);

                // remove the current marker and disable the corresponding evaluation icon
                _currentVisualMarker = null;
                _visualMarkerEvaluationIcon.gameObject.SetActive(false);
            }

            // if both markers are null and there was a collision before, reset
            // indicates the start of a new round
            if (_collisionDetected && _currentAudioMarker == null && _currentVisualMarker == null)
                _collisionDetected = false;
        }

        /// <summary>
        /// Start the evaluation of the matched markers.
        /// </summary>
        /// <param name="visualMarkerId">The id of the visual marker</param>
        /// <param name="audioMarkerId">The id of the audio marker</param>
        private void StartMatchEvaluation(int visualMarkerId, int audioMarkerId)
        {
            // the evaluation is skipped of there already is a detected collision 
            if (_collisionDetected)
                return;

            // set the end time (the calculation time is not regarded)
            _endTime = DateTime.Now;
            
            _collisionDetected = true;
            
            // evaluates the match
            StartCoroutine(EvaluateMatch(visualMarkerId, audioMarkerId));
        }

        /// <summary>
        /// Evaluate the found match.
        /// </summary>
        /// <param name="visualMarkerId">The id of the visual marker</param>
        /// <param name="audioMarkerId">The id of the audio marker</param>
        private IEnumerator EvaluateMatch(int visualMarkerId, int audioMarkerId)
        {
            // Debug.Log("Evaluating Match");
            _amountOfComparisons++;
                
            // set evaluation icon's position between both cards
            var vector = _imageTargetBehaviours[audioMarkerId].transform.position - _imageTargetBehaviours[visualMarkerId].transform.position;
            _audioMarkerEvaluationIcon.position = _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward + 0.5f * vector;

            // disable the object of the visual marker
            _instances[visualMarkerId].gameObject.SetActive(false);

            // if the markers are a match
            if (_matchingMarkerIDs[audioMarkerId] == visualMarkerId)
            {
                // set the material of the evaluation icon
                _audioMarkerEvaluationIconRenderer.material = validMatchMaterial;
                
                // show evaluation result
                _audioMarkerEvaluationIcon.gameObject.SetActive(true);

                // play match sound (was null when the scene was reset, work-around)
                if (_audioManager.IsUnityNull())
                    _audioManager = FindObjectOfType<AudioManager>();
                _audioManager.Play("match_correct");

                // remove match from list
                _matchingMarkerContentsCopy.RemoveAll(match =>
                    match.AudioId == audioMarkerId && match.VisualId == visualMarkerId);
                
                // show remove cards icons
                StartCoroutine(RemoveCardsFromPlayField(visualMarkerId, audioMarkerId));
            }

            // if the markers are no match
            else
            {
                // set the material of the evaluation icon
                _audioMarkerEvaluationIconRenderer.material = invalidMatchMaterial;

                // show evaluation result
                _audioMarkerEvaluationIcon.gameObject.SetActive(true);

                // play no match sound
                _audioManager.Play("match_false");

                // show return cards to players icons
                StartCoroutine(ReturnCardsToPlayers(visualMarkerId, audioMarkerId));
            }
            
            yield return null;
        }

        /// <summary>
        /// Show icons that motivate the players to remove the cards from the play field.
        /// </summary>
        /// <param name="visualMarkerId">The id of the visual marker</param>
        /// <param name="audioMarkerId">The id of the audio marker</param>
        private IEnumerator RemoveCardsFromPlayField(int visualMarkerId, int audioMarkerId)
        {
            // Debug.Log("Cards Match!");

            // wait so that visual player can see that it was a match
            yield return new WaitForSeconds(1.5f);

            // Debug.Log("Remove cards from play field!");

            // set positions of icons
            // _audioMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[audioMarkerId].transform);
            _audioMarkerEvaluationIcon.position = _imageTargetBehaviours[audioMarkerId].transform.position + 0.5f * Vector3.forward;

            // _visualMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[visualMarkerId].transform);
            _visualMarkerEvaluationIcon.position = _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward;
            _visualMarkerEvaluationIcon.gameObject.SetActive(true);

            // show RemoveCardsFromPlayFieldPrefab on top of both markers
            _audioMarkerEvaluationIconRenderer.material = removeCardsFromPlayFieldMaterial;
            _visualMarkerRenderer.material = removeCardsFromPlayFieldMaterial;

            // keeps updating the position of the icons
            StartCoroutine(UpdateIconPosition(audioMarkerId, visualMarkerId));
            
            // wait until both cards were removed from the play field
            while (_currentAudioMarker != null || _currentVisualMarker != null)
            {
                yield return new WaitForSeconds(1);
            }

            _collisionDetected = false;

            // if all matches were found notify game finished event
            if (_matchingMarkerContentsCopy.Count == 0)
            {
                GameEvents.instance.OnGameFinished(_amountOfComparisons, _endTime.Subtract(_startTime));
                yield return null;
            }
            
            yield return new WaitForSeconds(3);
        }
        
        /// <summary>
        /// Show icons that motivate the players to return the cards to the players.
        /// </summary>
        /// <param name="visualMarkerId">The id of the visual marker</param>
        /// <param name="audioMarkerId">The id of the audio marker</param>
        private IEnumerator ReturnCardsToPlayers(int visualMarkerId, int audioMarkerId)
        {
            // Debug.Log("Cards do not Match!");

            // wait so that visual player can see that it was no match
            yield return new WaitForSeconds(1.5f);

            // Debug.Log("Return cards to players!");

            // set positions of icons
            // _audioMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[audioMarkerId].transform);
            _audioMarkerEvaluationIcon.position =
                _imageTargetBehaviours[audioMarkerId].transform.position + 0.5f * Vector3.forward;

            // _visualMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[visualMarkerId].transform);
            _visualMarkerEvaluationIcon.position =
                _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward;
            _visualMarkerEvaluationIcon.gameObject.SetActive(true);

            // show ReturnCardsToPlayersPrefab on top of both markers
            _audioMarkerEvaluationIconRenderer.material = returnCardToAudioPlayerMaterial;
            _visualMarkerRenderer.material = returnCardToVisualPlayerMaterial;

            // keeps updating the position of the icons
            StartCoroutine(UpdateIconPosition(audioMarkerId, visualMarkerId));

            // wait until both cards were removed from the play field
            while (_currentAudioMarker != null || _currentVisualMarker != null)
            {
                yield return new WaitForSeconds(1);
            }

            _collisionDetected = false;
            yield return new WaitForSeconds(3);
        }

        /// <summary>
        /// Update the position of the icons.
        /// </summary>
        /// <param name="visualMarkerId">The id of the visual marker</param>
        /// <param name="audioMarkerId">The id of the audio marker</param>
        private IEnumerator UpdateIconPosition(int audioMarkerId, int visualMarkerId)
        {
            while (_currentAudioMarker != null && _currentVisualMarker != null)
            {
                // _audioMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[audioMarkerId].transform);
                _audioMarkerEvaluationIcon.position = _imageTargetBehaviours[audioMarkerId].transform.position + 0.5f * Vector3.forward;

                // _visualMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[visualMarkerId].transform);
                _visualMarkerEvaluationIcon.position = _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward;

                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}