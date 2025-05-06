using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UObj = UnityEngine.Object;

namespace InnerEigong;

internal static class CloneManager {
    private static Phantom _phantom;

    internal static void Initialize(GameObject refObj) {
        var phantomObj = UObj.Instantiate(refObj, refObj.transform.position, Quaternion.identity);
        _phantom = phantomObj.AddComponent<Phantom>();
    }

    private static CancellationTokenSource _spawnCancelSrc;

    internal static async UniTask SpawnPhantom(StealthGameMonster refMonster, float spawnDelaySec = 0.25f) {
        if (_spawnCancelSrc == null) {
            _spawnCancelSrc = new CancellationTokenSource();
            try {
                await _phantom.Spawn(refMonster, _spawnCancelSrc.Token, spawnDelaySec);
            }
            catch (OperationCanceledException) {
                await _phantom.DeSpawn();
            }
            finally {
                _spawnCancelSrc = null;
            }
        }
        else {
            _spawnCancelSrc.Cancel();
            _spawnCancelSrc = null;
        }
    }
}