using System;
using BepInEx;
using Cysharp.Threading.Tasks;
using RCGFSM.Animation;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace InnerEigong;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("NineSols.exe")]
public class Mod : BaseUnityPlugin {
    /// <summary>
    /// Static instance of the mod class.
    /// </summary>
    private static Mod _instance;

    private void Awake() {
        _instance = this;
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
    /// Log a message; for developer use.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Log(object message) {
        _instance.Logger.LogInfo(message);
    }
}