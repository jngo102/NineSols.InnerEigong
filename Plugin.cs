using BepInEx;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace InnerEigong;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("NineSols.exe")]
public class Plugin : BaseUnityPlugin {
    /// <summary>
    /// Static instance of the plugin class.
    /// </summary>
    internal static Plugin Instance { get; private set; }

    private void Awake() {
        Instance = this;
        PatchManager.Initialize();
    }

#if DEBUG
    private void Start() {
        const string bossScene = "A11_S0_Boss_YiGung";
        SceneManager.sceneLoaded += async (scene, _) => {
            switch (scene.name) {
                case "Logo":
                    while (SceneManager.GetActiveScene().name == "Logo") {
                        SkippableManager.Instance.TrySkip();
                    }

                    break;
                case "TitleScreenMenu":
                    StartMenuLogic.Instance.StartMemoryChallenge();
                    break;
                case bossScene:
                    SkippableManager.Instance.TrySkip();
                    break;
                default:
                    var gameCore = GameCore.Instance;
                    await UniTask.WaitUntil(() => gameCore.currentCoreState == GameCore.GameCoreState.Playing);
                    gameCore.GoToScene(bossScene);
                    break;
            }
        };
    }
#endif

    /// <summary>
    /// Log a message; for developer use.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void Log(object message) {
        Logger.LogInfo(message);
    }
}