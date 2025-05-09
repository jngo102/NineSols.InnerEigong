﻿using Cysharp.Threading.Tasks;
using HarmonyLib;
using I2.Loc;

// ReSharper disable InconsistentNaming

namespace InnerEigong;

/// <summary>
/// Patches methods for the Inner Eigong mod.
/// </summary>
internal class InnerEigongPatches {
    /// <summary>
    /// Add <see cref="Eigong">custom behavior</see> to the Eigong boss.
    /// </summary>  
    [HarmonyPrefix]
    [HarmonyPatch(typeof(StealthGameMonster), "Awake")]
    private static void CheckForEigong(StealthGameMonster __instance) {
        if (__instance.gameObject.name != Constants.BossName) return;
        PhantomManager.Initialize(__instance.gameObject);
        __instance.TryGetCompOrAdd<Eigong>();
        // __instance.AddComp(typeof(FireTrail));
        // SpriteManager.Initialize(Constants.BossSpritePrefix).Forget();
    }

    /// <summary>
    /// Spawn a <see cref="Phantom">phantom</see> on certain attacks.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MonsterBase), nameof(MonsterBase.ChangeStateIfValid), typeof(MonsterBase.States),
        typeof(MonsterBase.States))]
    private static async void CheckSpawnPhantom(MonsterBase __instance) {
        if (__instance.name != Constants.BossName) return;
#if !DEBUG
        // In release builds, halve the change that a phantom spawns on certain attacks; always spawn
        // a phantom during these attacks in debug builds
        var rand = new System.Random((int)UnityEngine.Time.timeSinceLevelLoad);
        if (rand.Next(2) != 0) return;
#endif
        var state = __instance.fsm.State;
        if (__instance is StealthGameMonster refMonster &&
            state is MonsterBase.States.Attack1 or MonsterBase.States.Attack3 or MonsterBase.States.Attack4
                or MonsterBase.States.Attack6 or MonsterBase.States.Attack12 or MonsterBase.States.Attack13
                or MonsterBase.States.Attack18) {
            await PhantomManager.SpawnPhantoms(refMonster);
        }
    }

    /// <summary>
    /// Clear <see cref="FireTrail">fire trail</see> during <see cref="TeleportBinding.Teleport">teleportation</see>.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeleportBinding), nameof(TeleportBinding.Teleport))]
    private static void ResetFlamesOnTeleport(TeleportBinding __instance) {
        var fireTrail = __instance.transform.root.GetComponentInChildren<FireTrail>();
        fireTrail?.ResetFlames();
    }

    /// <summary>
    /// Updates the <see cref="LocalizationManager.GetTranslation">translation</see> for Eigong's name.
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
            "es-ES" or "es-US" => $"{__result} interior",
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
    /// <see cref="Health.BecomeInvincible">Force invincibility</see> for debug purposes.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Health), nameof(Health.RemoveInvincible))]
    private static void ForceInvincible(Health __instance) {
        var player = __instance.transform.root.GetComponentInChildren<Player>();
        if (player) __instance.BecomeInvincible(player);
    }
#endif
}