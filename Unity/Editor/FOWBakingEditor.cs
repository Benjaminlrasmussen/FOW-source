using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FOW
{
    class FOWBakingEditor : EditorWindow
    {
        private string scene = null;
        private int visionMask = 1;
        private int camoMask = 0;
        private Terrain terrain = null;
        private Vector2 scrollPosition = default(Vector2);

        private string lastScene = null;

        private const string InfoPath = "Assets/FOW/";
        private const string BakedDataPath = "Assets/FOW/Resources/BakedData/";
        private const string BakedDataResPath = "BakedData/";
        private const string LevelBakingInfoPath = "Assets/FOW/Resources/BakingInfo/";
        private const string FOWDataFileExtension = ".bytes";

        [MenuItem("Tools/FOW Baker")]
        private static void Init()
        {
            GetWindow<FOWBakingEditor>();
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey("FOWLayerMask")) visionMask = EditorPrefs.GetInt("FOWLayerMask");
            if (EditorPrefs.HasKey("CamoLayerMask")) camoMask = EditorPrefs.GetInt("CamoLayerMask");
            lastScene = SceneManager.GetActiveScene().name;

            Directory.CreateDirectory(InfoPath);
            Directory.CreateDirectory(BakedDataPath);
            Directory.CreateDirectory(LevelBakingInfoPath);
        }

        private void OnGUI()
        {
            scene = SceneManager.GetActiveScene().name;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fog of War Baking Tool", EditorStyles.largeLabel);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            LayerMask tempVisionMask = EditorGUILayout.MaskField("Vision Blocking Layers: ", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(visionMask), InternalEditorUtility.layers);
            visionMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempVisionMask);

            GUILayout.Space(5);

            LayerMask tempCamoMask = EditorGUILayout.MaskField("Camouflage layers: ", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(camoMask), InternalEditorUtility.layers);
            camoMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempCamoMask);

            if (scene != lastScene)
            {
                lastScene = scene;
                terrain = null;
            }

            FOWLevelBakingInfo levelInfo = AssetDatabase.LoadAssetAtPath<FOWLevelBakingInfo>(LevelBakingInfoPath + scene + ".Asset");

            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.5f));
            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (levelInfo != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Baking Info:", EditorStyles.boldLabel);
                if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                {
                    AssetDatabase.DeleteAsset(LevelBakingInfoPath + scene + ".Asset");
                    return;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                levelInfo.unsafeMode = EditorGUILayout.Toggle("Unsafe mode:", levelInfo.unsafeMode);
                GUILayout.Space(5);

                if (levelInfo.unsafeMode)
                {
                    levelInfo.sceneName = EditorGUILayout.TextField("Scene name:", levelInfo.sceneName);
                    levelInfo.mapSize = EditorGUILayout.IntField("Map size:", levelInfo.mapSize);
                    levelInfo.eyeHeight = EditorGUILayout.FloatField("Eye height:", levelInfo.eyeHeight);
                    levelInfo.maxVisionRange = EditorGUILayout.FloatField("Max vision range:", levelInfo.maxVisionRange);
                    levelInfo.tileSize = EditorGUILayout.IntField("Tile size:", levelInfo.tileSize);
                    levelInfo.maxTerrainHeight = EditorGUILayout.FloatField("Max terrain height:", levelInfo.maxTerrainHeight);
                    levelInfo.terrainOffset = EditorGUILayout.Vector3Field("Terrain offset:", levelInfo.terrainOffset);
                }
                else
                {
                    int maxMapSize = 128 * levelInfo.tileSize;
                    int minMapSize = 2 * levelInfo.tileSize;

                    levelInfo.sceneName = EditorGUILayout.TextField("Scene name:", levelInfo.sceneName);
                    levelInfo.mapSize = EditorGUILayout.IntSlider("Map size:", levelInfo.mapSize, minMapSize, maxMapSize);
                    levelInfo.eyeHeight = EditorGUILayout.Slider("Eye height:", levelInfo.eyeHeight, 0.05f, 10);
                    levelInfo.maxVisionRange = EditorGUILayout.Slider("Max vision range:", levelInfo.maxVisionRange, 1, 150);
                    levelInfo.tileSize = EditorGUILayout.IntSlider("Tile size:", levelInfo.tileSize, 1, 4);

                    if (levelInfo.tileSize == 3) levelInfo.tileSize = 4;

                    levelInfo.maxTerrainHeight = EditorGUILayout.Slider("Max terrain height:", levelInfo.maxTerrainHeight, 1, 200);
                    levelInfo.terrainOffset = EditorGUILayout.Vector3Field("Terrain offset:", levelInfo.terrainOffset);
                }

                GUILayout.Space(20);

                GUILayout.Label("Baked Data:", EditorStyles.boldLabel);
                levelInfo.filePath = EditorGUILayout.TextField("File path:", levelInfo.filePath);

                bool tileMapMismatch = levelInfo.mapSize % levelInfo.tileSize != 0;

                if (!File.Exists(levelInfo.filePath))
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("No file found in file path. Bake new?", EditorStyles.boldLabel);

                    EditorGUI.BeginDisabledGroup(tileMapMismatch);
                    if (GUILayout.Button("Bake", GUILayout.ExpandWidth(false)))
                    {
                        FOWBaker baker = new FOWBaker(levelInfo);
                        baker.BakeAndSaveData(visionMask, camoMask);
                        AssetDatabase.Refresh();
                    }
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();

                    if (tileMapMismatch)
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("Map size must be dividable by tile size.", EditorStyles.miniBoldLabel);
                    }

                    GUILayout.Space(5);
                }
                else
                {
                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();
                    long fileSize = new FileInfo(levelInfo.filePath).Length / 1024 / 1024;
                    GUILayout.Label("Size: " + fileSize + " MB");

                    EditorGUI.BeginDisabledGroup(tileMapMismatch);
                    if (GUILayout.Button("Re-bake", GUILayout.ExpandWidth(false)))
                    {
                        FOWBaker baker = new FOWBaker(levelInfo);
                        baker.BakeAndSaveData(visionMask, camoMask);
                        AssetDatabase.Refresh();
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                    {
                        File.Delete(levelInfo.filePath);
                        AssetDatabase.Refresh();
                    }
                    GUILayout.EndHorizontal();

                    if (tileMapMismatch)
                    {
                        GUILayout.Space(30);
                        GUILayout.Label("Map size must be dividable by tile size.", EditorStyles.miniBoldLabel);
                    }

                    GUILayout.Space(5);
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No baking info for scene '" + scene + "'", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                terrain = EditorGUILayout.ObjectField("Terrain:", terrain, typeof(Terrain), true) as Terrain;

                EditorGUI.BeginDisabledGroup(terrain == null);
                if (GUILayout.Button("Add data", GUILayout.ExpandWidth(false)))
                {
                    levelInfo = CreateLevelBakingInfo(scene + ".Asset");
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (GUI.changed)
            {
                EditorPrefs.SetInt("FOWLayerMask", visionMask);
                EditorPrefs.SetInt("CamoLayerMask", camoMask);
                if (levelInfo != null)
                {
                    EditorPrefs.SetFloat("LastEyeHeight", levelInfo.eyeHeight);
                    EditorPrefs.SetFloat("LastVisionRange", levelInfo.maxVisionRange);
                }
                EditorUtility.SetDirty(levelInfo);
            }
        }

        private FOWLevelBakingInfo CreateLevelBakingInfo(string name)
        {
            FOWLevelBakingInfo asset = CreateInstance<FOWLevelBakingInfo>();
            asset.unsafeMode = false;
            asset.sceneName = scene;
            asset.mapSize = (int)terrain.terrainData.size.x;
            asset.eyeHeight = EditorPrefs.HasKey("LastEyeHeight") ? EditorPrefs.GetFloat("LastEyeHeight") : 1;
            asset.maxVisionRange = EditorPrefs.HasKey("LastVisionRange") ? EditorPrefs.GetFloat("LastVisionRange") : 50;
            asset.tileSize = 2;
            asset.maxTerrainHeight = terrain.terrainData.size.y;
            asset.terrainOffset = terrain.transform.position;
            asset.filePath = BakedDataPath + asset.sceneName + FOWDataFileExtension;
            asset.resourcePath = BakedDataResPath + asset.sceneName;

            AssetDatabase.CreateAsset(asset, LevelBakingInfoPath + name);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
