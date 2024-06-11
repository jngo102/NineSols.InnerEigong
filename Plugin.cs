using BepInEx;

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

    /// <summary>
    /// Log a message; for developer use.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void Log(object message) {
        Logger.LogInfo(message);
    }
}