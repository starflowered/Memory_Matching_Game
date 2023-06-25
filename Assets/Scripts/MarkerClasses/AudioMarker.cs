using UnityEngine;

namespace MarkerClasses
{
    public class AudioMarker : Marker
    {
        private AudioClip _soundClip;

        public AudioMarker(int id, AudioClip soundClip) : base(id)
        {
            _soundClip = soundClip;
        }
    }
}