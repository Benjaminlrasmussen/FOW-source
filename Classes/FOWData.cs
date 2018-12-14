using System;
using System.Collections.Generic;
using UnityEngine;

namespace FOW
{
    public class FOWData : IFOWData
    {
        private readonly Vector2[] neighbours;

        private int mapSize;
        private int tileSize;
        private float terrainHeight;
        private Vector3 terrainOffset;
        private Dictionary<Point2D, IFOWNode> nodes;
        private Dictionary<Point2D, float> camoNodes;

        public FOWData(int mapSize, int tileSize, float terrainHeight, Vector3 offset, Dictionary<Point2D, IFOWNode> nodes, Dictionary<Point2D, float> camoNodes)
        {
            this.mapSize = mapSize;
            this.terrainHeight = terrainHeight;
            this.tileSize = tileSize;
            this.terrainOffset = offset;
            this.nodes = nodes;
            this.camoNodes = camoNodes;

            neighbours = new Vector2[]
            {
                new Vector2(0, tileSize),
                new Vector2(0, -tileSize),
                new Vector2(-tileSize, 0),
                new Vector2(tileSize, 0),
                new Vector2(-tileSize, -tileSize),
                new Vector2(tileSize, -tileSize),
                new Vector2(-tileSize, tileSize),
                new Vector2(tileSize, tileSize)
            };
        }

        public IFOWNode GetNode(Point2D point)
        {
            return nodes.ContainsKey(point) ? nodes[point] : null;
        }

        public IFOWNode GetNode(byte x, byte y)
        {
            return GetNode(new Point2D(x, y));
        }

        public Point2D ClampToPoint(float x, float z, bool correctOffset = true)
        {
            if (correctOffset)
            {
                x = Mathf.Abs(terrainOffset.x - x);
                z = Math.Abs(terrainOffset.z - z);
            }

            byte xb = (byte)(x / tileSize);
            byte zb = (byte)(z / tileSize);
            return new Point2D(xb, zb);
        }

        public bool FindValidPoint(float x, float z, out Point2D point)
        {
            Vector2 pos = new Vector2(x - terrainOffset.x, z - terrainOffset.z);

            float minDist = float.MaxValue;
            Point2D? chosen = null;
            for (int i = 0; i < neighbours.Length; i++)
            {
                Vector2 next = pos + neighbours[i];
                Point2D p = ClampToPoint(next.x, next.y, false);
                if (ValidPoint(p))
                {
                    next.x = p.x * tileSize + tileSize / 2;
                    next.y = p.y * tileSize + tileSize / 2;

                    float dist = Vector2.Distance(pos, next);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        chosen = p;
                    }
                }
            }

            if (chosen.HasValue)
            {
                point = chosen.Value;
                return true;
            }

            point = new Point2D(0, 0);
            return false;
        }

        public bool ValidPoint(Point2D point)
        {
            return nodes.ContainsKey(point);
        }

        public float GetCamoValue(float x, float z)
        {
            Point2D point = ClampToPoint(x, z);
            return camoNodes.ContainsKey(point) ? camoNodes[point] : 0;
        }

        public void Serialize(IDataOut dataOut)
        {
            dataOut.Write(mapSize);
            dataOut.Write(tileSize);
            dataOut.Write(terrainHeight);
            dataOut.Write(terrainOffset.x);
            dataOut.Write(terrainOffset.y);
            dataOut.Write(terrainOffset.z);

            dataOut.Write(nodes.Count);
            foreach (KeyValuePair<Point2D, IFOWNode> pair in nodes)
            {
                pair.Key.Serialize(dataOut);
                pair.Value.Serialize(dataOut);
            }

            dataOut.Write(camoNodes.Count);
            foreach (KeyValuePair<Point2D, float> pair in camoNodes)
            {
                pair.Key.Serialize(dataOut);
                dataOut.Write(pair.Value);
            }
        }

        public static FOWData DeSerialize(IDataIn dataIn)
        {
            int mapSize = dataIn.ReadInt();
            int tileSize = dataIn.ReadInt();
            float terrainHeight = dataIn.ReadFloat();
            Vector3 offset = new Vector3(dataIn.ReadFloat(), dataIn.ReadFloat(), dataIn.ReadFloat());

            int nodeCount = dataIn.ReadInt();
            Dictionary<Point2D, IFOWNode> nodes = new Dictionary<Point2D, IFOWNode>(nodeCount);

            for (int i = 0; i < nodeCount; i++)
            {
                Point2D key = Point2D.DeSerialize(dataIn);
                FOWNode value = FOWNode.DeSerialize(dataIn);
                nodes.Add(key, value);
            }

            int camoCount = dataIn.ReadInt();
            Dictionary<Point2D, float> camoNodes = new Dictionary<Point2D, float>(camoCount);

            for (int i = 0; i < camoCount; i++)
            {
                Point2D key = Point2D.DeSerialize(dataIn);
                float value = dataIn.ReadFloat();
                camoNodes.Add(key, value);
            }

            return new FOWData(mapSize, tileSize, terrainHeight, offset, nodes, camoNodes);
        }

        public int LengthInTiles
        {
            get { return mapSize / tileSize; }
        }

        public int MapSize
        {
            get { return mapSize; }
        }

        public int TileSize
        {
            get { return tileSize; }
        }

        public float TerrainHeight
        {
            get { return terrainHeight; }
        }

        public Vector3 Offset
        {
            get { return terrainOffset; }
        }
    }
}
