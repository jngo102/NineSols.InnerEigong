using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UObj = UnityEngine.Object;

namespace InnerEigong;

/// <summary>
/// Manages the phantom clone of a monster.
/// </summary>
internal static class PhantomManager {
    private static Phantom _phantom;

    /// <summary>
    /// Initialize the phantom manager.
    /// </summary>
    /// <param name="refObj">A reference object that the phantom will be a clone of and that it will spawn to.</param>
    internal static void Initialize(GameObject refObj) {
        var phantomObj = UObj.Instantiate(refObj, refObj.transform.position, Quaternion.identity);
        _phantom = phantomObj.AddComponent<Phantom>();
    }

    private static CancellationTokenSource _spawnCancelSrc;

    /// <summary>
    /// Spawn a phantom.
    /// </summary>
    /// <param name="refMonster">The monster to spawn the phantom at.</param>
    /// <param name="spawnDelaySec">The duration in seconds before which the phantom will actually spawn.</param>
    /// <returns></returns>
    internal static async UniTask SpawnPhantom(MonsterBase refMonster, float spawnDelaySec = 0.25f) {
        if (_spawnCancelSrc == null) {
            _spawnCancelSrc = new CancellationTokenSource();
            try {
                await _phantom.Spawn(refMonster, _spawnCancelSrc.Token, spawnDelaySec);
            }
            catch (OperationCanceledException) {
                await _phantom.DeSpawn(_spawnCancelSrc.Token);
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