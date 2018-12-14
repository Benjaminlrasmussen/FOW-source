using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using UnityEngine;

namespace FOW
{
    class ColorArrayUpdateWorker
    {
        private Dictionary<IFOWAgent, AgentInfo> agentViews;
        private IFOWData data;
        private int texSize;

        private Color[] colors;
        private TextureScaler scaler;
        private Color[] result;

        private bool running;
        private float time;
        private bool dirty;

        public bool Locked { get; private set; }

        public ColorArrayUpdateWorker(Dictionary<IFOWAgent, AgentInfo> agentViews, IFOWData data, int texSize)
        {
            this.agentViews = agentViews;
            this.data = data;
            this.texSize = texSize;

            colors = new Color[texSize * texSize];

            Color defaultColor = new Color(0f, 0.0f, 0f, 1f);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = defaultColor;
            }
        }

        public void Start()
        {
            if (running) return;

            running = true;
            new Thread(Run).Start();
        }

        public void Stop()
        {
            dirty = true;
            running = false;
        }

        public Color[] GetData()
        {
            if (Locked) throw new ThreadStateException();
            return result;
        }

        public void FeedData(float time)
        {
            this.time = time;
            Locked = true;
            dirty = true;
        }

        public void SetRemovedAgentColors(AgentInfo info)
        {
            RemoveOldColors(info.lastData, info.lastLength, 1f);
        }

        private void Run()
        {
            while (running)
            {
                while (!dirty) Thread.Sleep(5);

                foreach (IFOWAgent agent in agentViews.Keys)
                {
                    AgentInfo info = agentViews[agent];
                    if (info.lastData == null) continue;
                    SetAlphaForNodes(info.lastData, info.lastLength, 1f);
                }

                foreach (IFOWAgent agent in agentViews.Keys)
                {
                    AgentInfo info = agentViews[agent];
                
                    Point2D last = info.lastPoint;
                    IFOWNode node = data.GetNode(last);

                    if (node == null) continue;

                    int length = node.GetVisibleLength(agent.VisionRange);
                    info.lastLength = length;

                    if (length == 0) continue;

                    ReadOnlyCollection<Point2D> points = node.Points;
                    SetAlphaForNodes(points, length, 0f);
                    info.lastData = points;
                }

                result = colors;

                Locked = false;
                dirty = false;
            }
        }

        private void SetAlphaForNodes(ReadOnlyCollection<Point2D> nodes, int length, float alpha)
        {
            for (int i = 0; i < length; i++)
            {
                Point2D p = nodes[i];
                int index = p.x + texSize * p.y;
                colors[index].a = alpha;
            }
        }

#region DISTANCE IN COLOR
        private void RemoveOldColors(ReadOnlyCollection<Point2D> nodes, int length, float alpha)
        {
            for (int i = 0; i < length; i++)
            {
                Point2D p = nodes[i];
                int index = p.x + texSize * p.y;
                colors[index].a = alpha;
                colors[index].r = 0f;
            }
        }

        private void AddNewColors(ReadOnlyCollection<Point2D> nodes, int length, float alpha, Point2D originPoint)
        {
            for (int i = 0; i < length; i++)
            {
                Point2D p = nodes[i];
                int index = p.x + texSize * p.y;
                colors[index] = GetDistanceColor(p, originPoint, alpha, index);
            }
        }

        private Color GetDistanceColor(Point2D targetPoint, Point2D originPoint, float alpha, int index)
        {
            Color color = new Color(0f, 0f, 0f, alpha);

            float difX = targetPoint.x - originPoint.x;
            float difY = targetPoint.y - originPoint.y;

            float distance = Mathf.Sqrt((difX * difX) + (difY * difY));
            float normalizedDistance = Mathf.Min(distance / 15f, 1f);
            float finalDistance = 0.3f + (0.4f * normalizedDistance);

            if (colors[index].r == 0f)
            {
                color.r = 1f;
                color.g = finalDistance;
            }
            else
            {
                color.g = Mathf.Min(colors[index].g, finalDistance);
            }

            return color;
        }
#endregion

#region TIMESTAMP IN COLOR
        private Color EncodeTimeAsColor(float time, byte alpha)
        {
            Color enc = new Color();

            int b = (int)(time / 10000);
            time -= b * 10000;

            int g = (int)(time / 100);
            time -= g * 100;

            enc.a = alpha;
            enc.b = (float)b / 100;
            enc.g = (float)g / 100;
            enc.r = time / 100;

            return enc;
        }
#endregion
    }
}
