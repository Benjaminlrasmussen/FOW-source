using System.Collections.ObjectModel;

namespace FOW
{
    public interface IFOWNode : ISerializable
    {
        int GetVisibleLength(float maxDistance);
        bool CanSee(Point2D point, float maxDistance);
        ReadOnlyCollection<Point2D> Points { get; }
    }
}
