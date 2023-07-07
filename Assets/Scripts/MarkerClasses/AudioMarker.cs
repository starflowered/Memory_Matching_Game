using Vuforia;

namespace MarkerClasses
{
    /// <summary>
    /// This class is used as a data container for audio marker information. 
    /// </summary>
    public class AudioMarker : Marker
    {
        #region properties

        public string SoundClipName { get; }

        #endregion

        #region constructors

        public AudioMarker(int id, ImageTargetBehaviour imageTargetBehaviour, string soundClipName) : base(id, imageTargetBehaviour)
        {
            SoundClipName = soundClipName;
        }

        #endregion
        
        public override string ToString()
        {
            return $"|audio marker {Id}|";
        }
    }
}