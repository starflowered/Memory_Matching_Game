using UnityEngine;

namespace MarkerClasses
{
    public class VisualMarker : Marker
    {
        private GameObject _objectModel;

        public VisualMarker(int id, GameObject objectModel) : base(id)
        {
            _objectModel = objectModel;
        }
    }
}