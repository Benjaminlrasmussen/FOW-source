using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FOW
{
    public class FOWNode : IFOWNode
    {
        private static readonly Comparer comp = new Comparer();

        private ReadOnlyCollection<Point2D> points;
        private byte[] nodeDistances;

        public FOWNode(List<Pair<byte, Point2D>> visibleNodes)
        {
            Point2D[] nodeArray = new Point2D[visibleNodes.Count];
            nodeDistances = new byte[visibleNodes.Count];

            visibleNodes.Sort(comp);
            for (int i = 0; i < visibleNodes.Count; i++)
            {
                nodeArray[i] = visibleNodes[i].right;
                nodeDistances[i] = visibleNodes[i].left;
            }

            points = new ReadOnlyCollection<Point2D>(nodeArray);
        }

        public FOWNode(Point2D[] points, byte[] nodeDistances)
        {
            this.nodeDistances = nodeDistances;
            this.points = new ReadOnlyCollection<Point2D>(points);
        }

        public int GetVisibleLength(float maxDistance)
        {
            int min = 0;
            int max = nodeDistances.Length;

            while (min != max)
            {
                int middle = (min + max) / 2;

                if (middle == points.Count - 1) return nodeDistances.Length;
                if (nodeDistances[middle] <= maxDistance)
                {
                    if (nodeDistances[middle + 1] > maxDistance) return middle + 1;
                    min = middle + 1;
                }
                else
                {
                    max = middle;
                }
            }

            return 0;
        }

        // Could be faster //
        public bool CanSee(Point2D point, float maxDistance)
        {
            int index = points.IndexOf(point);
            return index > -1 ? nodeDistances[index] <= maxDistance : false;
        }

        public void Serialize(IDataOut dataOut)
        {
            dataOut.Write(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                dataOut.Write(nodeDistances[i]);
                points[i].Serialize(dataOut);
            }
        }

        public static FOWNode DeSerialize(IDataIn dataIn)
        {
            int length = dataIn.ReadInt();
            Point2D[] points = new Point2D[length];
            byte[] distances = new byte[length];

            for (int i = 0; i < length; i++)
            {
                byte dist = dataIn.ReadByte();
                Point2D point = Point2D.DeSerialize(dataIn);
                distances[i] = dist;
                points[i] = point;
            }

            return new FOWNode(points, distances);
        }

        public ReadOnlyCollection<Point2D> Points { get { return points; } }

        private class Comparer : IComparer<Pair<byte, Point2D>>
        {
            public int Compare(Pair<byte, Point2D> x, Pair<byte, Point2D> y)
            {
                return x.left - y.left;
            }
        }
    }
}
