using UnityEngine;
using Vuforia;

namespace MarkerClasses
{
    /// <summary>
    /// This class is used as a data container for visual marker information. 
    /// </summary>
    public class VisualMarker : Marker
    {
        #region properties

        public GameObject ObjectModel { get; }

        // instantiated _objectModel
        public GameObject PlacedObject { get; set; }

        #endregion

        #region constructors

        public VisualMarker(int id, ImageTargetBehaviour imageTargetBehaviour, GameObject objectModel) : base(id, imageTargetBehaviour)
        {
            ObjectModel = objectModel;
        }

        #endregion
        
        public override string ToString()
        {
            return $"|visual marker {Id}|";
        }
    }
}