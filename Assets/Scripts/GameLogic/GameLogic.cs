using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using MarkerClasses;
using Unity.VisualScripting;
using UnityEngine;
using Vuforia;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class GameLogic : MonoBehaviour
    {
        #region constants

        private const string DatabasePath = "Vuforia/Colored_Markers.xml";
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

        [Header("UI")]
        [SerializeField] [Tooltip("UI Element that shows the evaluation progress")] 
        private GameObject progressUI;
        
        [SerializeField] [Tooltip("The UI's image that shows the total progress amount")]
        private Image progressAmountImage;

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
        private Renderer _audioMarkerRenderer;
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

        private List<MatchingMarkerContents> _matchingMarkerContentsCopy;
        private int _amountOfComparisons;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _timeTookToFindAllMatches;
        
        #endregion

        #region unity event methods

        private void OnEnable()
        {
            VuforiaApplication.Instance.OnVuforiaStarted += InitAll;
        }

        private void OnDisable()
        {
            VuforiaApplication.Instance.OnVuforiaStarted -= InitAll;
            GameEvents.instance.matched -= StartMatchEvaluation;
        }

        private void Start()
        {
#if !UNITY_EDITOR
            // set debug mode to false if not in unity editor
            debug = false;
#endif

            _audioManager = FindObjectOfType<AudioManager>();

            _audioMarkerEvaluationIcon = Instantiate(matchEvaluationPrefab).transform;
            _audioMarkerEvaluationIcon.gameObject.SetActive(false);
            _audioMarkerRenderer = _audioMarkerEvaluationIcon.GetComponent<UpdateRotation>().Renderer;

            _visualMarkerEvaluationIcon = Instantiate(matchEvaluationPrefab).transform;
            _visualMarkerEvaluationIcon.gameObject.SetActive(false);
            _visualMarkerRenderer = _visualMarkerEvaluationIcon.GetComponent<UpdateRotation>().Renderer;

            GameEvents.instance.matched += StartMatchEvaluation;

            _startTime = DateTime.Now;
        }

        #endregion

        private void InitAll()
        {
            SpawnVisualMarkerModels();

            InitMarkerIds();
        }

        private void SpawnVisualMarkerModels()
        {
            // init variables
            _imageTargetBehaviours = new Dictionary<int, ImageTargetBehaviour>();
            _colliders = new Dictionary<int, Collider>();
            _visualImageTargetBehaviours = new List<ImageTargetBehaviour>();
            _audioImageTargetBehaviours = new List<ImageTargetBehaviour>();
            
            // create image targets for each input marker
            for (var i = 1; i <= matchingMarkerContents.Count; i++) 
                CreateInstantImageTarget($"visualMarker0{i}", false);

            for (var i = 1; i <= matchingMarkerContents.Count; i++) 
                CreateInstantImageTarget($"audioMarker0{i}", true);

            CreateDebugImageTarget();
        }

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
            
            _matchingMarkerContentsCopy = new List<MatchingMarkerContents>(matchingMarkerContents);
        }

        private void CreateInstantImageTarget(string markerName, bool isAudioMarker)
        {
            Debug.Log($"Adding target {markerName}");
            
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
                case Status.EXTENDED_TRACKED:
                default:
                case Status.NO_POSE:

                    OnTargetLost(_imageTargetBehaviours[marker.ID.Value]);
                    break;

                case Status.LIMITED:
                case Status.TRACKED:

                    OnTargetFound(_imageTargetBehaviours[marker.ID.Value]);
                    break;
            }
        }

        private void OnTargetFound(ImageTargetBehaviour marker)
        {
            // check if the marker has an ID
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }

            var id = marker.ID.Value;

            _colliders[id].enabled = true;

            if (_idToMarkerDictionary[id].GetType() == typeof(AudioMarker))
            {
                if (!_currentAudioMarker.IsUnityNull())
                    return;

                // set recognized audio marker
                _currentAudioMarker = (AudioMarker)_idToMarkerDictionary[id];

                Debug.Log("Found Audio marker");

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

        private void DisplayMarkerModel(int id)
        {
            // enable spawned game object
            _instances[id].SetActive(true);

            // start rotating the object
            StartCoroutine(RotateModel(_instances[id], RotationSpeed));
        }

        private static IEnumerator RotateModel(GameObject instance, float rotationSpeed)
        {
            // rotate around self 
            while (instance.activeSelf)
            {
                instance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                yield return null;
            }
        }

        private void PlayMarkerSound()
        {
            _audioManager.Play(_currentAudioMarker.SoundClipName);
        }

        private void OnTargetLost(ImageTargetBehaviour marker)
        {
            // check if the marker has an ID
            if (!marker.ID.HasValue)
            {
                Debug.LogError($"Marker {marker.name} has no ID!");
                return;
            }

            var id = marker.ID.Value;

            _colliders[id].enabled = false;

            if (_idToMarkerDictionary[id].GetType() == typeof(AudioMarker))
            {
                _currentAudioMarker = null;
                _audioMarkerEvaluationIcon.gameObject.SetActive(false);
            }

            else
            {
                _instances[id].SetActive(false);

                _currentVisualMarker = null;
                _visualMarkerEvaluationIcon.gameObject.SetActive(false);
            }

            if (_collisionDetected && _currentAudioMarker == null && _currentVisualMarker == null)
                _collisionDetected = false;
        }

        private void StartMatchEvaluation(int visualMarkerId, int audioMarkerId)
        {
            if (_collisionDetected)
            {
                Debug.Log("There is a collision, evaluation is skipped");
                return;
            }

            // if (_currentAudioMarker.IsUnityNull() || _currentVisualMarker.IsUnityNull())
            // {
            //     Debug.Log(
            //         $"The current markers are null, evaluation is skipped ({_currentAudioMarker} | {_currentVisualMarker})");
            //     return;
            // }

            _endTime = DateTime.Now;
            _collisionDetected = true;
            StartCoroutine(EvaluateMatch(visualMarkerId, audioMarkerId));
        }

        private IEnumerator EvaluateMatch(int visualMarkerId, int audioMarkerId)
        {
            // progressUI.SetActive(true);
            // _audioMarkerEvaluationIcon.SetParent(null);
            // _visualMarkerEvaluationIcon.SetParent(null);
            //
            // const float evaluationTime = 3f;
            // const float progressAmount = 1f / evaluationTime;
            // var totalProgress = 0f;
            // for (var i = 0; i < evaluationTime; i++)
            // {
            //     // show circle timer progress
            //     progressAmountImage.fillAmount = totalProgress;
            //
            //     // make progress
            //     totalProgress += progressAmount;
            //     yield return new WaitForSeconds(1);
            // }

            Debug.Log("Evaluating Match");
            _amountOfComparisons++;
                
            // set evaluation icon's position between both cards
            var vector = _imageTargetBehaviours[audioMarkerId].transform.position - _imageTargetBehaviours[visualMarkerId].transform.position;
            _audioMarkerEvaluationIcon.position = _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward + 0.5f * vector;

            _instances[visualMarkerId].gameObject.SetActive(false);

            if (_matchingMarkerIDs[audioMarkerId] == visualMarkerId)
            {
                // markers match
                _audioMarkerRenderer.material = validMatchMaterial;
                
                // show evaluation result
                _audioMarkerEvaluationIcon.gameObject.SetActive(true);

                // play match sound
                if (_audioManager.IsUnityNull())
                    _audioManager = FindObjectOfType<AudioManager>();
                _audioManager.Play("match_correct");

                // remove match from list
                _matchingMarkerContentsCopy.RemoveAll(match =>
                    match.AudioId == audioMarkerId && match.VisualId == visualMarkerId);
                
                StartCoroutine(RemoveCardsFromPlayField(visualMarkerId, audioMarkerId));
            }

            else
            {
                // markers do not match
                _audioMarkerRenderer.material = invalidMatchMaterial;

                // show evaluation result
                _audioMarkerEvaluationIcon.gameObject.SetActive(true);

                // play no match sound
                _audioManager.Play("match_false");

                StartCoroutine(ReturnCardsToPlayers(visualMarkerId, audioMarkerId));
            }

            progressUI.SetActive(false);
            progressAmountImage.fillAmount = 0;
            
            yield return null;
        }

        private IEnumerator RemoveCardsFromPlayField(int visualMarkerId, int audioMarkerId)
        {
            Debug.Log("Cards Match!");

            // wait so that visual player can see that it was a match
            yield return new WaitForSeconds(1.5f);

            Debug.Log("Remove cards from play field!");

            // set positions of icons
            // _audioMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[audioMarkerId].transform);
            _audioMarkerEvaluationIcon.position = _imageTargetBehaviours[audioMarkerId].transform.position + 0.5f * Vector3.forward;

            // _visualMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[visualMarkerId].transform);
            _visualMarkerEvaluationIcon.position = _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward;
            _visualMarkerEvaluationIcon.gameObject.SetActive(true);

            // show RemoveCardsFromPlayFieldPrefab on top of both markers
            _audioMarkerRenderer.material = removeCardsFromPlayFieldMaterial;
            _visualMarkerRenderer.material = removeCardsFromPlayFieldMaterial;

            StartCoroutine(UpdateIconPosition(audioMarkerId, visualMarkerId));
            
            while (_currentAudioMarker != null || _currentVisualMarker != null)
            {
                yield return new WaitForSeconds(1);
            }

            _collisionDetected = false;

            if (_matchingMarkerContentsCopy.Count == 0)
            {
                GameEvents.instance.OnGameFinished(_amountOfComparisons, _endTime.Subtract(_startTime));
                yield return null;
            }
            
            yield return new WaitForSeconds(3);
        }
        
        private IEnumerator ReturnCardsToPlayers(int visualMarkerId, int audioMarkerId)
        {
            Debug.Log("Cards do not Match!");

            // wait so that visual player can see that it was no match
            yield return new WaitForSeconds(1.5f);

            Debug.Log("Return cards to players!");

            // set positions of icons
            // _audioMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[audioMarkerId].transform);
            _audioMarkerEvaluationIcon.position =
                _imageTargetBehaviours[audioMarkerId].transform.position + 0.5f * Vector3.forward;

            // _visualMarkerEvaluationIcon.SetParent(_imageTargetBehaviours[visualMarkerId].transform);
            _visualMarkerEvaluationIcon.position =
                _imageTargetBehaviours[visualMarkerId].transform.position + 0.5f * Vector3.forward;
            _visualMarkerEvaluationIcon.gameObject.SetActive(true);

            // show ReturnCardsToPlayersPrefab on top of both markers
            _audioMarkerRenderer.material = returnCardToAudioPlayerMaterial;
            _visualMarkerRenderer.material = returnCardToVisualPlayerMaterial;

            StartCoroutine(UpdateIconPosition(audioMarkerId, visualMarkerId));

            while (_currentAudioMarker != null || _currentVisualMarker != null)
            {
                yield return new WaitForSeconds(1);
            }

            _collisionDetected = false;
            yield return new WaitForSeconds(3);
        }

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