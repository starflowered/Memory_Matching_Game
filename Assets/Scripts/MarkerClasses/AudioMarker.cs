using UnityEngine;
using Vuforia;

namespace MarkerClasses
{
    public class AudioMarker : Marker
    {
        public AudioClip SoundClip { get; }

        public int ID => Id;

        public ImageTargetBehaviour ImageTargetBehaviour1 => ImageTargetBehaviour;
        
        public AudioMarker(int id, ImageTargetBehaviour imageTargetBehaviour, AudioClip soundClip) : base(id, imageTargetBehaviour)
        {
            SoundClip = soundClip;
        }
    }
}