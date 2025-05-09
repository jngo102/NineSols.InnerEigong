using HarmonyLib;
using System.Reflection;

namespace InnerEigong;

/// <summary>
/// Handles <see href="https://harmony.pardeike.net/articles/patching.html">patching</see> and unpatching of methods in the main <see cref="Assembly">assembly</see>.
/// </summary>
internal static class PatchManager {
    /// <summary>
    /// An instance of <see cref="Harmony" />.
    /// </summary>
    private static Harmony _harmony;

    /// <summary>
    /// Initialize all <see cref="InnerEigongPatches">patches</see>.
    /// </summary>
    public static void Patch() {
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(typeof(InnerEigongPatches));
    }

    /// <summary>
    /// <see cref="Harmony.Unpatch(MethodBase, HarmonyPatchType, string)">Unpatch</see> all methods patched by <see cref="InnerEigongPatches" /> in the <see cref="_harmony">Harmony instance</see>.
    /// </summary>
    public static void Unpatch() {
        var methodInfos = typeof(InnerEigongPatches).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
        foreach (var info in methodInfos) {
            _harmony.Unpatch(info, HarmonyPatchType.All, MyPluginInfo.PLUGIN_GUID);
        }
    }
}