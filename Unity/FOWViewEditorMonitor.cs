using System.IO;
using UnityEngine;

namespace FOW
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(FOWView))]
    public class FOWViewEditorMonitor : MonoBehaviour
    {
        private const string LevelBakingInfoPath = "BakingInfo/";

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
                return;
            }

            string scene = gameObject.scene.name;
            FOWLevelBakingInfo info = Resources.Load<FOWLevelBakingInfo>(LevelBakingInfoPath + scene);
            if (info != null)
            {
                if (!File.Exists(info.filePath))
                {
                    Debug.LogError("There is no baked FOW data for the scene (" + scene + "). Path: " + info.filePath);
                }
            }
            else
            {
                Debug.LogError("There is no level baking info for this scene (" + scene + "). Go to Tools>FOW Baker to add data.");
            }
        }
    }
}
