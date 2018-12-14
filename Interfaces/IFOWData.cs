
namespace FOW
{
    public interface IFOWData : ISerializable
    {
        IFOWNode GetNode(Point2D point);
        IFOWNode GetNode(byte x, byte y);
    }
}
