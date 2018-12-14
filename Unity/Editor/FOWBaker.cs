using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;

namespace FOW
{
    public class FOWBaker
    {
        private const string IgnoreFOWTilingTag = "FOWIgnoreTileHeight";
        private readonly FOWLevelBakingInfo info;

        public FOWBaker(FOWLevelBakingInfo info)
        {
            this.info = info;
        }

        public void BakeAndSaveData(int layerMask, int camoMask)
        {
            EditorUtility.DisplayProgressBar("FOW", "Baking fog of war. Please wait...", 0);

            FOWData data = Bake(layerMask, camoMask);

            if (data != null)
            {
                EditorUtility.DisplayProgressBar("FOW", "Saving the data...", 1);

                IDataOut persister = new PersistenceWriter(info.filePath);
                data.Serialize(persister);
                persister.Close();
            }

            EditorUtility.ClearProgressBar();
        }

        private FOWData Bake(int layerMask, int camoMask)
        {
            int size = info.mapSize * info.mapSize / info.tileSize / info.tileSize;
            RaycastHit[] hits = new RaycastHit[size];
            Point2D[] points = new Point2D[size];
            float tileHalf = (float)info.tileSize / 2;
            Dictionary<Point2D, Pair<float, Collider>> tempCamoNodes = new Dictionary<Point2D, Pair<float, Collider>>();

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(IgnoreFOWTilingTag);
            SetActiveBatch(gameObjects, false);
            bool cancel = FindTiles(hits, points, tempCamoNodes, tileHalf, layerMask, camoMask);
            SetActiveBatch(gameObjects, true);

            if (cancel) return null;

            EditorUtility.DisplayProgressBar("FOW", "Baking fog of war. Please wait...", 0.01f);

            Dictionary<Point2D, IFOWNode> nodes = new Dictionary<Point2D, IFOWNode>();

            cancel = ConnectTiles(hits, points, nodes, tempCamoNodes, tileHalf, layerMask);
            if (cancel) return null;

            Dictionary<Point2D, float> camoNodes = new Dictionary<Point2D, float>(tempCamoNodes.Count);
            foreach (KeyValuePair<Point2D, Pair<float, Collider>> pair in tempCamoNodes)
            {
                camoNodes.Add(pair.Key, pair.Value.left);
            }

            return new FOWData(info.mapSize, info.tileSize, info.maxTerrainHeight, info.terrainOffset, nodes, camoNodes);
        }

        private bool FindTiles(RaycastHit[] hits, Point2D[] points, Dictionary<Point2D, Pair<float, Collider>> tempCamoNodes, float tileHalf, int layerMask, int camoMask)
        {
            Vector3 originY = Vector3.up * (info.maxTerrainHeight + 1);
            float rayDist = originY.magnitude + 1;
            originY += info.terrainOffset;
            RaycastHit hit = default(RaycastHit);
            int pointer = 0;

            for (int x = 0; x < info.mapSize / info.tileSize; x++)
            {
                for (int z = 0; z < info.mapSize / info.tileSize; z++)
                {
                    Vector3 origin = new Vector3(x * info.tileSize + tileHalf, 0, z * info.tileSize + tileHalf) + originY;
                    if (!Physics.Raycast(origin, Vector3.down, out hit, rayDist, layerMask))
                    {
                        Debug.LogError("Something went wrong when baking FOW while trying to hit world." +
                            " Make sure the map size is correct, and that the terrain has a Vision Blocking Layer.");
                        return true;
                    }

                    Point2D p = new Point2D((byte)x, (byte)z);
                    RaycastHit camoHit = default(RaycastHit);

                    if (Physics.Raycast(origin, Vector3.down, out camoHit, rayDist, camoMask, QueryTriggerInteraction.Collide))
                    {
                        ICamoArea area = camoHit.collider.GetComponentInParent<ICamoArea>();
                        if (area != null) tempCamoNodes.Add(p, new Pair<float, Collider>(area.GetCamoSpotReduction(), camoHit.collider));
                    }

                    points[pointer] = p;
                    hits[pointer] = hit;
                    pointer++;
                }
            }

            return false;
        }

        private bool ConnectTiles(RaycastHit[] hits, Point2D[] points, Dictionary<Point2D, IFOWNode> nodes, Dictionary<Point2D, Pair<float, Collider>> tempCamoNodes, float tileHalf, int layerMask)
        {
            const int MaxBatches = 5;
            NavMeshHit navHit = default(NavMeshHit);
            List<Pair<byte, Point2D>> visibleNodes = new List<Pair<byte, Point2D>>();

            List<RaycastCommand> rayCommands = new List<RaycastCommand>();
            Queue<RaycastJobHolder> rayJobs = new Queue<RaycastJobHolder>(5);

            for (int i = 0; i < hits.Length; i++)
            {
                Vector3 current = hits[i].point;

                float progress = Mathf.Clamp(0.01f + ((float)i / hits.Length) * 0.98f, 0, 1);
                bool cancel = EditorUtility.DisplayCancelableProgressBar("FOW", "Baking fog of war. Please wait...", progress);

                if (cancel)
                {
                    while (rayJobs.Count > 0)
                    {
                        RaycastJobHolder tempHolder = rayJobs.Dequeue();
                        tempHolder.Complete();
                        tempHolder.Dispose();
                    }

                    return true;
                }

                if (!NavMesh.SamplePosition(current, out navHit, tileHalf, NavMesh.AllAreas))
                {
                    tempCamoNodes.Remove(points[i]);
                    continue;
                }

                current.y += info.eyeHeight;

                while (rayJobs.Count == MaxBatches)
                {
                    while (rayJobs.Count > 0 && rayJobs.Peek().IsCompleted)
                    {
                        RaycastJobHolder currentHolder = rayJobs.Dequeue();
                        ProcessRaycastData(currentHolder, visibleNodes, nodes);
                    }

                    Thread.Sleep(1);
                }

                List<Pair<byte, Point2D>> nodePoints = new List<Pair<byte, Point2D>>();

                for (int j = 0; j < hits.Length; j++)
                {
                    Vector3 next = hits[j].point;
                    next.y += info.eyeHeight;

                    float dist = Vector3.Distance(current, next);
                    Vector3 dir = (next - current).normalized;

                    float camoDist = 0;
                    if (tempCamoNodes.ContainsKey(points[j]))
                    {
                        if (tempCamoNodes.ContainsKey(points[i]))
                        {
                            Pair<float, Collider> currentPair = tempCamoNodes[points[i]];
                            Pair<float, Collider> nextPair = tempCamoNodes[points[j]];

                            if (currentPair.right != nextPair.right)
                            {
                                camoDist = nextPair.left;
                            }
                        }
                        else
                        {
                            camoDist = tempCamoNodes[points[j]].left;
                        }
                    }

                    int roundDist = Mathf.RoundToInt(dist + camoDist);
                    if (roundDist > info.maxVisionRange) continue;

                    rayCommands.Add(new RaycastCommand(current, dir, dist, layerMask));
                    rayCommands.Add(new RaycastCommand(next, -dir, dist, layerMask));
                    nodePoints.Add(new Pair<byte, Point2D>((byte)roundDist, points[j]));
                }

                RaycastJobHolder holder = new RaycastJobHolder(rayCommands, nodePoints, points[i]);
                holder.Schedule(rayCommands.Count / 5);
                rayJobs.Enqueue(holder);
                rayCommands.Clear();
            }

            while (rayJobs.Count > 0)
            {
                RaycastJobHolder holder = rayJobs.Dequeue();
                ProcessRaycastData(holder, visibleNodes, nodes);
            }

            return false;
        }

        private void ProcessRaycastData(RaycastJobHolder holder, List<Pair<byte, Point2D>> visibleNodes, Dictionary<Point2D, IFOWNode> nodes)
        {
            holder.Complete();

            NativeArray<RaycastHit> results = holder.GetResults();
            List<Pair<byte, Point2D>> localPoints = holder.GetPoints();
            for (int j = 0; j < localPoints.Count; j++)
            {
                int index = j * 2;
                if (results[index].collider == null || results[index + 1].collider == null)
                {
                    visibleNodes.Add(localPoints[j]);
                }
            }

            nodes.Add(holder.OriginPoint, new FOWNode(visibleNodes));
            holder.Dispose();
            visibleNodes.Clear();
        }

        private void SetActiveBatch(GameObject[] gameObjects, bool active)
        {
            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(active);
            }
        }

        private class RaycastJobHolder
        {
            private NativeArray<RaycastCommand> rayCommands;
            private NativeArray<RaycastHit> results;
            private List<Pair<byte, Point2D>> points;
            private JobHandle handle;
            private Point2D originPoint;

            public RaycastJobHolder(List<RaycastCommand> inCommands, List<Pair<byte, Point2D>> points, Point2D originPoint)
            {
                this.points = points;
                this.originPoint = originPoint;

                rayCommands = new NativeArray<RaycastCommand>(inCommands.Count, Allocator.Temp);
                results = new NativeArray<RaycastHit>(inCommands.Count, Allocator.Temp);

                for (int i = 0; i < inCommands.Count; i++)
                {
                    rayCommands[i] = inCommands[i];
                }
            }

            public NativeArray<RaycastHit> GetResults()
            {
                return results;
            }

            public List<Pair<byte, Point2D>> GetPoints()
            {
                return points;
            }

            public void Schedule(int minCommandsPerJob)
            {
                handle = RaycastCommand.ScheduleBatch(rayCommands, results, minCommandsPerJob);
            }

            public void Complete()
            {
                handle.Complete();
            }

            public void Dispose()
            {
                rayCommands.Dispose();
                results.Dispose();
            }

            public bool IsCompleted
            {
                get { return handle.IsCompleted; }
            }

            public Point2D OriginPoint
            {
                get { return originPoint; }
            }
        }
    }
}
