using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreeEyedGames;

namespace FOW
{
    public class FOWView : MonoBehaviour
    {
        private static FOWView instance;

        private FilterMode filterMode = FilterMode.Trilinear;
        [SerializeField] private Color fogColor = new Color(0, 0, 0, 0.75f);
        [SerializeField] private Decal decal = null;
        private float fadeInSpeed = 3.5f;
        private float fadeOutSpeed = 2.5f;
        [SerializeField] private float padding = 0.0f;

        private FOWData data;

        private Dictionary<IFOWAgent, AgentInfo> agentViews;
        private Queue<Pair<IFOWAgent, bool>> addRemoveQueue;

        private Texture2D texture;

        private RenderTexture render;
        private RenderTexture previousRender;
        private RenderTexture upscaleRender;
        private RenderTexture blurRender;
        private RenderTexture secondBlurRender;

        private Material fadeMat;
        private Material upscaleMat;
        private Material blurMat;

        private bool updateNeeded;
        private bool isActive = false;
        private bool cachedSettingUseFOW = true;

        private Color lastColor;
        private FilterMode lastFilter;

        private ColorArrayUpdateWorker updater;

        private void Awake()
        {
            string scene = gameObject.scene.name;
            FOWLevelBakingInfo info = Resources.Load<FOWLevelBakingInfo>("BakingInfo/" + scene);

            if (info == null)
            {
                Debug.LogError("FOW view couldn't be initialized (Missing baking info)");
                Destroy(gameObject);
                return;
            }

            TextAsset asset = Resources.Load<TextAsset>(info.resourcePath);

            if (asset == null)
            {
                Debug.LogError("FOW view couldn't be initialized (Missing data: " + info.filePath);
                Destroy(gameObject);
                return;
            }

            PersistenceReader reader = new PersistenceReader(asset.bytes);
            data = FOWData.DeSerialize(reader);

            Debug.Log("FOW data loaded!");

            agentViews = new Dictionary<IFOWAgent, AgentInfo>();
            addRemoveQueue = new Queue<Pair<IFOWAgent, bool>>();

            SetupMaterials();
            SetupTransforms(info);

            lastColor = fogColor;
            lastFilter = filterMode;

            updater = new ColorArrayUpdateWorker(agentViews, data, data.LengthInTiles);
            updater.Start();

            StartCoroutine(UpdateRoutine());
            instance = this;
            ShowFog(false);
        }

        private void Update()
        {
            Settings_Video settings = Settings.Get<Settings_Video>();
            if (cachedSettingUseFOW != settings.showFogOfWar.value)
            {
                instance.decal.gameObject.SetActive(!cachedSettingUseFOW && isActive);
                cachedSettingUseFOW = settings.showFogOfWar.value;
            }

            if (isActive && cachedSettingUseFOW)
            {
                float fadeInStep = Mathf.Min(fadeInSpeed * Time.unscaledDeltaTime, 1f);
                float fadeOutStep = Mathf.Min(fadeOutSpeed * Time.unscaledDeltaTime, 1f);

                fadeMat.SetFloat("_FadeInStep", fadeInStep);
                fadeMat.SetFloat("_FadeOutStep", fadeOutStep);

                Graphics.Blit(texture, upscaleRender, upscaleMat);

                Graphics.Blit(upscaleRender, render, fadeMat);
                Graphics.Blit(render, previousRender);

                Graphics.Blit(render, blurRender, blurMat);
                Graphics.Blit(blurRender, secondBlurRender, blurMat);
            }
        }

        private void SetupTransforms(FOWLevelBakingInfo info)
        {
            //position
            Vector3 position = new Vector3(data.MapSize / 2, data.TerrainHeight / 2, data.MapSize / 2);
            position += data.Offset;

            //a lot of terrain goes slightly below 0.0, so we lower everything a bit to solve most of these cases
            position.y -= 1f;

            transform.position = position;
            transform.localEulerAngles = Vector3.zero;

            //decal scale
            Vector3 decalScale = new Vector3(data.MapSize, info.maxTerrainHeight, data.MapSize);
            decalScale.x += padding * 2;
            decalScale.z += padding * 2;

            decal.transform.localScale = decalScale;
            decal.transform.localPosition = Vector3.zero;
            decal.transform.localEulerAngles = Vector3.zero;

            //decal texture tiling
            float tiling = ((padding * 2) + data.MapSize) / data.MapSize;
            decal.Material.SetTextureScale("_MainTex", new Vector2(tiling, tiling));

            //decal texture offset
            float offset = padding / ((padding * 2) + data.MapSize);
            offset *= -tiling;

            //the upcaling logic ends up offsetting the output by half a tile in each direction
            offset += -((1f / data.LengthInTiles) * 0.5f);

            decal.Material.SetTextureOffset("_MainTex", new Vector2(offset, offset));
        }

        private void SetupMaterials()
        {
            int size = data.LengthInTiles;

            texture = new Texture2D(size, size, TextureFormat.RGBAHalf, false);
            render = new RenderTexture(texture.width * 4, texture.height * 4, 0);
            previousRender = new RenderTexture(texture.width * 4, texture.height * 4, 0);
            upscaleRender = new RenderTexture(texture.width * 4, texture.height * 4, 0);
            blurRender = new RenderTexture(texture.width * 4, texture.height * 4, 0);
            secondBlurRender = new RenderTexture(texture.width * 4, texture.height * 4, 0);

            fadeMat = new Material(Shader.Find("Hidden/FadeFOW"));
            upscaleMat = new Material(Shader.Find("Hidden/Upscale4x"));
            blurMat = new Material(Shader.Find("Hidden/Blur5x5"));

            texture.filterMode = FilterMode.Point;
            render.filterMode = FilterMode.Point;
            previousRender.filterMode = FilterMode.Point;
            upscaleRender.filterMode = FilterMode.Point;
            blurRender.filterMode = FilterMode.Point;
            secondBlurRender.filterMode = filterMode;

            //Clone the material to avoid changing the material asset
            decal.Material = new Material(decal.Material);
            decal.Material.SetColor("_Color", fogColor);
            decal.Material.mainTexture = secondBlurRender;

            fadeMat.SetTexture("_PreviousTex", previousRender);
        }

        private IEnumerator UpdateRoutine()
        {
            YieldInstruction wait = new WaitForEndOfFrame();

            while (true)
            {
                if (!isActive || !cachedSettingUseFOW)
                {
                    yield return wait;
                    continue;
                }

                if (!fogColor.Equals(lastColor))
                {
                    lastColor = fogColor;
                    decal.Material.SetColor("_Color", fogColor);
                    yield return UpdateAgentViews(wait);
                }
                else if (filterMode != lastFilter)
                {
                    lastFilter = filterMode;
                    secondBlurRender.filterMode = filterMode;
                }

                foreach (KeyValuePair<IFOWAgent, AgentInfo> pair in agentViews)
                {
                    IFOWAgent agent = pair.Key;

                    AgentInfo info = pair.Value;
                    float x = agent.PosX;
                    float z = agent.PosZ;
                    Point2D point = data.ClampToPoint(x, z);

                    bool valid = !data.ValidPoint(point) ? data.FindValidPoint(x, z, out point) : true;

                    if (!point.Equals(info.lastPoint) && valid)
                    {
                        info.lastPoint = point;
                        updateNeeded = true;
                    }
                }

                HandleChangedAgents();

                if (updateNeeded) yield return UpdateAgentViews(wait);

                yield return wait;
            }
        }

        private IEnumerator UpdateAgentViews(YieldInstruction wait)
        {
            updateNeeded = false;
            updater.FeedData(Time.unscaledTime);

            while (updater.Locked) yield return wait;

            texture.SetPixels(updater.GetData());
            texture.Apply(false);
        }

        private void HandleChangedAgents()
        {
            while (addRemoveQueue.Count > 0)
            {
                Pair<IFOWAgent, bool> pair = addRemoveQueue.Dequeue();
                IFOWAgent agent = pair.left;

                if (pair.right)
                {
                    Point2D current = data.ClampToPoint(agent.PosX, agent.PosZ);
                    agentViews.Add(agent, new AgentInfo(current));
                }
                else
                {
                    AgentInfo info = agentViews.ContainsKey(agent) ? agentViews[agent] : null;
                    if (info != null) updater.SetRemovedAgentColors(info);
                    agentViews.Remove(agent);
                }

                updateNeeded = true;
            }
        }

        private void OnDisable()
        {
            if (updater != null) updater.Stop();
        }

        public static void RegisterAgent(IFOWAgent agent)
        {
            instance.addRemoveQueue.Enqueue(new Pair<IFOWAgent, bool>(agent, true));
        }

        public static void UnRegisterAgent(IFOWAgent agent)
        {
            instance.addRemoveQueue.Enqueue(new Pair<IFOWAgent, bool>(agent, false));
        }

        public static void NotifyAgentVisionChange(IFOWAgent agent)
        {
            // Sub-optimal solution for now //
            instance.updateNeeded = true;
        }

        public static void ShowFog(bool show)
        {
            instance.isActive = show;
            instance.decal.gameObject.SetActive(show);
        }

        public static bool CanSee(float x1, float z1, float x2, float z2, float visionRange)
        {
            Point2D p1 = instance.data.ClampToPoint(x1, z1);
            Point2D p2 = instance.data.ClampToPoint(x2, z2);

            if (!instance.data.ValidPoint(p1))
            {
                bool found = instance.data.FindValidPoint(x1, z1, out p1);
                if (!found) return false;
            }
            if (!instance.data.ValidPoint(p2))
            {
                bool found = instance.data.FindValidPoint(x2, z2, out p2);
                if (!found) return false;
            }

            IFOWNode node = instance.data.GetNode(p1);
            return node.CanSee(p2, visionRange);
        }

        public static bool CanSee(IFOWAgent looker, IFOWAgent target)
        {
            return CanSee(looker.PosX, looker.PosZ, target.PosX, target.PosZ, looker.VisionRange);
        }

        public static float GetCamoValue(float x, float z)
        {
            return instance.data.GetCamoValue(x, z);
        }

        public static float GetCamoValue(IFOWAgent agent)
        {
            return GetCamoValue(agent.PosX, agent.PosZ);
        }

        public static Point2D ClampToPoint(float x, float z)
        {
            return instance.data.ClampToPoint(x, z);
        }

        public static bool ValidPoint(float x, float z)
        {
            Point2D p = instance.data.ClampToPoint(x, z);
            return instance.data.ValidPoint(p);
        }

        public static bool FindValidPoint(float x, float z, out Point2D point)
        {
            return instance.data.FindValidPoint(x, z, out point);
        }
    }
}
