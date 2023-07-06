using UnityEngine;
using Vuforia;

namespace MarkerClasses
{
    public class VisualMarker : Marker
    {
        public GameObject ObjectModel { get; }

        // instantiated _objectModel
        public GameObject PlacedObject { get; set; }
        
        public VisualMarker(int id, ImageTargetBehaviour imageTargetBehaviour, GameObject objectModel) : base(id, imageTargetBehaviour)
        {
            ObjectModel = objectModel;
        }
        
        public override string ToString()
        {
            return $"|visual marker {Id}|";
        }
    }
}