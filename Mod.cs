using System.IO;
using System.Reflection;
using BepInEx;
using Cysharp.Threading.Tasks;
using RCGFSM.Animation;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace InnerEigong;

/// <summary>
/// The main <see href="https://docs.bepinex.dev">BepInEx</see> mod <see href="https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html">plugin</see>.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("NineSols.exe")]
public class Mod : BaseUnityPlugin {
    /// <summary>
    /// Static instance of the <see cref="Mod">mod</see> class.
    /// </summary>
    private static Mod _instance;

    /// <summary>
    /// The <see cref="Texture2D">texture</see> to change the boss's cloak to. 
    /// </summary>
    internal static Texture2D CloakTexture { get; private set; }

    /// <summary>
    /// Color key <see cref="Shader">shader</see> that will be applied to all <see cref="SpriteRenderer">sprite renderers</see> on the boss.
    /// </summary>
    internal static Shader ColorKeyShader { get; private set; }
    
    /// <summary>
    /// Color key <see cref="Material">material</see> that will be applied to all <see cref="SpriteRenderer">sprite renderers</see> on the boss.
    /// </summary>
    internal static Material ColorKeyMaterial { get; private set; }
    
    /// <summary>
    /// Tracking slashes game object.
    /// </summary>
    internal static GameObject TrackingSlashesGameObject { get; private set; }

    private void Awake() {
        _instance = this;
        LoadAssets();
        PatchManager.Patch();
    }

#if DEBUG
    private async void Start() {
        // Load Eigong scene immediately on game start
        RuntimeInitHandler.LoadCore();
        GameConfig.Instance.InstantiateGameCore();
        await UniTask.WaitUntil(() => SaveManager.Instance);
        var saveManager = SaveManager.Instance;
        saveManager.SavePlayerPref();
        await saveManager.LoadSaveAtSlot(4);
        await SceneManager.LoadSceneAsync(Constants.BossSceneName);
        // Skip the boss intro cutscene
        var fsmObj = GameObject.Find("General Boss Fight FSM Object Variant");
        var fsmOwner = fsmObj.TryGetComp<StateMachineOwner>();
        var fsmContext = fsmOwner.FsmContext;
        await UniTask.WaitUntil(() => fsmContext.currentStateType);
        var playAction = fsmContext.currentStateType.GetComponentInChildren<CutScenePlayAction>(true);
        if (playAction.cutscene is SimpleCutsceneManager cutsceneManager) {
            cutsceneManager.TrySkip();
        }
    }
#endif

    private void OnApplicationQuit() {
        PatchManager.Unpatch();
    }

    /// <summary>
    /// Load all assets in the embedded innereigong asset bundle.
    /// </summary>
    private static void LoadAssets() {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resourceName in assembly.GetManifestResourceNames()) {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;

            var bundle = AssetBundle.LoadFromStream(stream);
            CloakTexture = bundle.LoadAsset<Texture2D>("CloakTexture");
            ColorKeyShader = bundle.LoadAsset<Shader>("_2dxFX_ColorKeyOverlay");
            ColorKeyMaterial = bundle.LoadAsset<Material>("_2dxFX_ColorKeyOverlay");
            TrackingSlashesGameObject = bundle.LoadAsset<GameObject>("Tracking Slashes");
            TrackingSlashesGameObject.AddComponent<TrackingSlashes>();
        }
    }

    /// <summary>
    /// Log a message; for developer use.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Log(object message) {
        _instance.Logger.LogInfo(message);
    }
}