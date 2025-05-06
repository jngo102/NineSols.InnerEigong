using System;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace InnerEigong;

internal class Phantom : MonoBehaviour {
    private StealthGameMonster _monster;

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

        _monster = GetComponent<StealthGameMonster>();
        _monster.EnterLevelAwake();
        _monster.EnterLevelReset();
        _monster.fsm.Changed += OnStateChange;

        foreach (var rend in GetComponentsInChildren<SpriteRenderer>()) {
            if (rend.name.Contains("Shadow")) continue;
            rend.TryGetCompOrAdd<_2dxFX_ColorRGB>();
            rend.TryGetCompOrAdd<_2dxFX_Negative>();
        }

        _monster.Hide();
    }

    private void OnStateChange(MonsterBase.States state) { }

    private MonsterBase.States _currentState;

    internal async UniTask Spawn(StealthGameMonster refMonster, CancellationToken spawnCancelToken,
        float spawnDelaySeconds = 0.25f) {
        transform.position = refMonster.transform.position;
        _currentState = refMonster.CurrentState;
        await UniTask.Delay(TimeSpan.FromSeconds(spawnDelaySeconds), cancellationToken: spawnCancelToken);
        _monster.Show();
        _monster.health.SetReceiversActivate(false);
        _monster.health.BecomeInvincible(_monster);
        _monster.ChangeStateIfValid(_currentState);
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
        await UniTask.WaitUntil(() => _monster.CurrentState != _currentState, cancellationToken: spawnCancelToken);
        await DeSpawn();
    }

    internal async UniTask DeSpawn(float fadeTimeSec = 0.25f) {
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
        });
        _monster.Hide();
    }
}