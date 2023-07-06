using Vuforia;

namespace MarkerClasses
{
    public abstract class Marker
    {
        public int Id { get; }
        public ImageTargetBehaviour ImageTargetBehaviour { get; }

        protected Marker(int id, ImageTargetBehaviour imageTargetBehaviour)
        {
            Id = id;
            ImageTargetBehaviour = imageTargetBehaviour;
        }
    }
}
