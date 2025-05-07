using System;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

namespace InnerEigong;

/// <summary>
/// Patches methods for the Inner Eigong mod.
/// </summary>
internal class InnerEigongPatches {
    /// <summary>
    /// Add custom behavior to Eigong boss.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StealthGameMonster), "Awake")]
    private static void CheckForEigong(StealthGameMonster __instance) {
        if (__instance.gameObject.name != Constants.BossName) return;
        CloneManager.Initialize(__instance.gameObject);
        __instance.AddComp(typeof(Eigong));
        // __instance.AddComp(typeof(FireTrail));
        // SpriteManager.Initialize(Constants.BossSpritePrefix).Forget();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MonsterBase), nameof(MonsterBase.ChangeStateIfValid), typeof(MonsterBase.States),
        typeof(MonsterBase.States))]
    private static void SpawnPhantom(MonsterBase __instance) {
        if (__instance.name != Constants.BossName) return;
#if !DEBUG
        var rand = new System.Random((int)Time.timeSinceLevelLoad);
        if (rand.Next(2) != 0) return;
#endif
        var state = __instance.fsm.State;
        if (__instance is StealthGameMonster refMonster &&
            state is MonsterBase.States.Attack1 or MonsterBase.States.Attack3 or MonsterBase.States.Attack4
                or MonsterBase.States.Attack6 or MonsterBase.States.Attack12 or MonsterBase.States.Attack13
                or MonsterBase.States.Attack18) {
            CloneManager.SpawnPhantom(refMonster).Forget();
        }
    }

    /// <summary>
    /// Clear fire trail check during teleportation.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeleportBinding), nameof(TeleportBinding.Teleport))]
    private static void ResetFlamesOnTeleport(TeleportBinding __instance) {
        var fireTrail = __instance.transform.root.GetComponentInChildren<FireTrail>();
        fireTrail?.ResetFlames();
    }

    /// <summary>
    /// Updates the translation for Eigong's name.
    /// </summary>
    /// <param name="Term">The localization key term.</param>
    /// <param name="__result">The output string.</param>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.GetTranslation))]
    private static void GetInnerEigongTranslation(string Term, ref string __result) {
        if (Term != "Characters/NameTag_YiKong") return;
        __result = LocalizationManager.CurrentLanguageCode switch {
            "de-DE" => __result,
            "en-US" => $"Inner {__result}",
            "es-US" or "es-ES" => $"{__result} interior",
            "fr-FR  " => $"{__result}, Vision",
            "it" => $"{__result} interiore",
            "ja" => $"心中の{__result}",
            "ko" => $"마음속{__result}",
            "pl" => $"Wewnętrzny {__result}",
            "pt-BR" => $"{__result} Interior",
            "ru" => $"Иная {__result}",
            "uk" => $"Інакша {__result}",
            "zh-CN" or "zh-TW" => $"心中的{__result}",
            _ => __result
        };
    }

#if DEBUG
    /// <summary>
    /// Force invincibility for debug purposes.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Health), nameof(Health.RemoveInvincible))]
    private static void ForceInvincible(Health __instance) {
        var player = __instance.transform.root.GetComponentInChildren<Player>();
        if (player) __instance.BecomeInvincible(player);
    }
#endif
}