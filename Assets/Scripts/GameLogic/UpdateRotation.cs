using UnityEngine;

namespace GameLogic
{
    public class UpdateRotation : MonoBehaviour
    {
        private Transform _t;
        private Transform _mainCamera;

        private Renderer _renderer;

        public Renderer Renderer => _renderer;

        private void OnEnable()
        {
            _t = transform;

            if (!_t.GetChild(0).gameObject.TryGetComponent(out _renderer))
            {
                Debug.LogError($"Renderer was not found on game object {name}");
            }
        }

        private void Start()
        {
            if (Camera.main != null)
                _mainCamera = Camera.main.transform;
            else Debug.LogError("Main camera has not been initialized yet!");
            
            // _t.LookAt(_mainCamera);
        }

        private void Update()
        {
            // _t.LookAt(_mainCamera);
        }
    }
}