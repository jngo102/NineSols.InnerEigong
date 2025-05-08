using System;
using System.Reflection;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace InnerEigong;

/// <summary>
/// Represents an enemy doppelgänger that attacks alongside the actual boss.
/// </summary>
internal class Phantom : MonoBehaviour {
    private MonsterBase _monster;
    
    private void Awake() {
        var guid = gameObject.GetGuidComponent();
        var guidType = guid.GetType();
        var bytes = new byte[16];
        var random = new System.Random((int)Time.timeSinceLevelLoad);
        random.NextBytes(bytes);
        var newGuid = new Guid(bytes);
        guidType.GetField("guid", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(guid, newGuid);
        guidType.GetField("serializedGuid", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.SetValue(guid, newGuid.ToByteArray());
        guid.Invoke("CreateGuid", 0);

        AutoAttributeManager.AutoReferenceAllChildren(gameObject);

        TryGetComponent(out _monster);
        _monster.EnterLevelAwake();
        _monster.EnterLevelReset();

        foreach (var rend in GetComponentsInChildren<SpriteRenderer>()) {
            if (rend.name.Contains("Shadow")) continue;
            rend.TryGetCompOrAdd<_2dxFX_ColorRGB>();
            rend.TryGetCompOrAdd<_2dxFX_Negative>();
        }
        
        _monster.Hide();
    }

    /// <summary>
    /// Fade in the phantom at Eigong's position.
    /// </summary>
    /// <param name="refMonster">The original Eigong's monster component.</param>
    /// <param name="spawnCancelToken">A cancellation token that can stop the spawn task.</param>
    /// <param name="spawnDelaySeconds">The duration before when the clone actually spawns.</param>
    internal async UniTask Spawn(MonsterBase refMonster, CancellationToken spawnCancelToken,
        float spawnDelaySeconds = 0.25f) {
        transform.position = refMonster.transform.position;
        var currentState = refMonster.CurrentState;
        await UniTask.Delay(TimeSpan.FromSeconds(spawnDelaySeconds), cancellationToken: spawnCancelToken);
        _monster.Show();
        _monster.health.SetReceiversActivate(false);
        _monster.health.BecomeInvincible(_monster);
        _monster.ChangeStateIfValid(currentState);
        var fadeStartTime = Time.timeSinceLevelLoad;
        float alpha = 0;
        await UniTask.WaitUntil(() => {
            foreach (var fx in GetComponentsInChildren<_2dxFX_ColorRGB>(true)) {
                fx._Alpha = alpha;
            }

            foreach (var fx in GetComponentsInChildren<_2dxFX_Negative>(true)) {
                fx._Alpha = alpha;
            }

            alpha = (Time.timeSinceLevelLoad - fadeStartTime) / spawnDelaySeconds;
            return alpha >= 1;
        }, cancellationToken: spawnCancelToken);
        await UniTask.WaitUntil(() => _monster.CurrentState != currentState, cancellationToken: spawnCancelToken);
        await DeSpawn();
    }

    /// <summary>
    /// Fade out the phantom.
    /// </summary>
    /// <param name="fadeCancelToken">A cancellation token that may stop this fade out routine.</param>
    /// <param name="fadeTimeSec">The duration to fade out for.</param>
    internal async UniTask DeSpawn(CancellationToken fadeCancelToken, float fadeTimeSec = 0.25f) {
        float alpha = 1;
        float fadeStartTime = Time.timeSinceLevelLoad;
        await UniTask.WaitUntil(() => {
            foreach (var fx in GetComponentsInChildren<_2dxFX_ColorRGB>(true)) {
                fx._Alpha = alpha;
            }

            foreach (var fx in GetComponentsInChildren<_2dxFX_Negative>(true)) {
                fx._Alpha = alpha;
            }

            alpha = 1 - (Time.timeSinceLevelLoad - fadeStartTime) / fadeTimeSec;
            return alpha <= 0;
        }, cancellationToken: fadeCancelToken);
        _monster.Hide();
    }
}