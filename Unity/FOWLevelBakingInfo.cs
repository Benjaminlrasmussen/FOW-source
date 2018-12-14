using UnityEngine;

namespace FOW
{
    public class FOWLevelBakingInfo : ScriptableObject
    {
        public bool unsafeMode;
        public string sceneName;
        public int mapSize;
        public float eyeHeight;
        public float maxVisionRange;
        public int tileSize;
        public float maxTerrainHeight;
        public Vector3 terrainOffset;
        public string filePath;
        public string resourcePath;
    }
}
