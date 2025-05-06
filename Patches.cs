using Cysharp.Threading.Tasks;
using HarmonyLib;
using I2.Loc;
using System.Threading;
using UnityEngine;

namespace InnerEigong;

/// <summary>
///     Patches methods for the Inner Eigong mod.
/// </summary>
internal class InnerEigongPatches {
    /// <summary>
    ///     Add custom behavior to Eigong boss.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StealthGameMonster), "Awake")]
    private static void CheckForEigong(StealthGameMonster __instance) {
        if (__instance.gameObject.name != "Boss_Yi Gung") return;
        __instance.AddComp(typeof(Eigong));
        // __instance.AddComp(typeof(FireTrail));
        CloneManager.Initialize(__instance.gameObject);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MonsterBase), nameof(MonsterBase.ChangeStateIfValid), typeof(MonsterBase.States),
        typeof(MonsterBase.States))]
    private static void SpawnPhantom(MonsterBase __instance) {
        if (__instance.name != "Boss_Yi Gung") return;
        var rand = new System.Random((int)Time.timeSinceLevelLoad);
        if (rand.Next(2) != 0) return;
        var state = __instance.fsm.State;
        if (__instance is StealthGameMonster refMonster &&
            state is MonsterBase.States.Attack1 or MonsterBase.States.Attack3 or MonsterBase.States.Attack4
                or MonsterBase.States.Attack6 or MonsterBase.States.Attack12 or MonsterBase.States.Attack13
                or MonsterBase.States.Attack17 or MonsterBase.States.Attack18) {
            CloneManager.SpawnPhantom(refMonster).Forget();
        }
    }

    /// <summary>
    ///     Clear fire trail check during teleportation.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeleportBinding), nameof(TeleportBinding.Teleport))]
    private static void ResetFlamesOnTeleport(TeleportBinding __instance) {
        var fireTrail = __instance.transform.root.GetComponentInChildren<FireTrail>();
        fireTrail?.ResetFlames();
    }

    /// <summary>
    ///     Add the prefix "Inner " to the localized "Eigong" string.
    /// </summary>
    /// <param name="Term">The localization key term.</param>
    /// <param name="__result">The output string.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    private static void GetInnerEigongTranslation(string Term, ref string __result) {
        if (Term != "Characters/NameTag_YiKong") return;
        var prefix = LocalizationManager.CurrentLanguageCode switch {
            "ja" => "心中の",
            "ko" => "마음속",
            var lang when lang.Contains("zh") => "心中的",
            "en" or _ => "Inner "
        };
        __result = prefix + __result;
    }

#if DEBUG
    /// <summary>
    ///     Force invincibility for debug purposes.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Health), nameof(Health.RemoveInvincible))]
    private static void ForceInvincible(Health __instance) {
        var player = __instance.transform.root.GetComponentInChildren<Player>();
        if (player) __instance.BecomeInvincible(player);
    }
#endif
}