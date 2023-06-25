using UnityEngine;
using Vuforia;

namespace MarkerClasses
{
    public class AudioMarker : Marker
    {
        public AudioClip SoundClip { get; }

        public AudioMarker(int id, ImageTargetBehaviour imageTargetBehaviour, AudioClip soundClip) : base(id, imageTargetBehaviour)
        {
            SoundClip = soundClip;
        }
    }
}