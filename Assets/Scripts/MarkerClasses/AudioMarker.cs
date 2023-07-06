using Vuforia;

namespace MarkerClasses
{
    public class AudioMarker : Marker
    {
        public string SoundClipName { get; }

        public AudioMarker(int id, ImageTargetBehaviour imageTargetBehaviour, string soundClipName) : base(id, imageTargetBehaviour)
        {
            SoundClipName = soundClipName;
        }
        
        public override string ToString()
        {
            return $"|audio marker {Id}|";
        }
    }
}