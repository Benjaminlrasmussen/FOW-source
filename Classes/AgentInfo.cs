using System.Collections.ObjectModel;

namespace FOW
{
    class AgentInfo
    {
        public ReadOnlyCollection<Point2D> lastData;
        public int lastLength;
        public Point2D lastPoint;

        public AgentInfo(Point2D lastPoint)
        {
            this.lastPoint = lastPoint;
        }
    }
}
