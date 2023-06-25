using Vuforia;

namespace MarkerClasses
{
    public abstract class Marker
    {

        protected readonly int Id;

        protected readonly ImageTargetBehaviour ImageTargetBehaviour;

        protected Marker(int id, ImageTargetBehaviour imageTargetBehaviour)
        {
            Id = id;
            ImageTargetBehaviour = imageTargetBehaviour;
        }

    }
}
