using Vuforia;

namespace MarkerClasses
{
    /// <summary>
    /// This class is used as a data container for marker information. 
    /// </summary>
    public abstract class Marker
    {
        #region properties

        public int Id { get; }

        public ImageTargetBehaviour ImageTargetBehaviour { get; }
        
        #endregion

        #region constructors

        protected Marker(int id, ImageTargetBehaviour imageTargetBehaviour)
        {
            Id = id;
            ImageTargetBehaviour = imageTargetBehaviour;
        }

        #endregion
    }
}
