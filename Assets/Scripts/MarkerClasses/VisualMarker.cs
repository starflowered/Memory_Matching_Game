using UnityEngine;
using Vuforia;

namespace MarkerClasses
{
    public class VisualMarker : Marker
    {
        public GameObject ObjectModel { get; }

        // instantiated _objectModel
        public GameObject PlacedObject { get; set; }
        public int ID => Id;

        public ImageTargetBehaviour ImageTargetBehaviour1 => ImageTargetBehaviour;
        
        public VisualMarker(int id, ImageTargetBehaviour imageTargetBehaviour, GameObject objectModel) : base(id, imageTargetBehaviour)
        {
            ObjectModel = objectModel;
        }
    }
}