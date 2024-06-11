using HarmonyLib;
using System.Reflection;

namespace InnerEigong;
/// <summary>
/// Handles patching and unpatching of methods in the main Assembly.
/// </summary>
internal static class PatchManager {
    /// <summary>
    /// An instance of Harmony.
    /// </summary>
    private static Harmony _harmony;

    /// <summary>
    /// Initialize the patch manager.
    /// </summary>
    public static void Initialize() {
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(typeof(InnerEigongPatches));
    }

    /// <summary>
    /// Unpatch all methods patched by BossRushPatches in the Harmony instance.
    /// </summary>
    public static void Unpatch() {
        var methodInfos = typeof(InnerEigongPatches).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        foreach (var info in methodInfos) {
            _harmony.Unpatch(info, HarmonyPatchType.All, MyPluginInfo.PLUGIN_GUID);
        }
    }
}