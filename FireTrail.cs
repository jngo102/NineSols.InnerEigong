using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PoolManager;

namespace InnerEigong;

/// <summary>
/// Leaves a trail of fire at the actor's feet.
/// </summary>
internal class FireTrail : MonoBehaviour {
    public float flameSpacing = 35;
    public float groundY = -1100;

    private Collider2D _collider;
    private ObjectPool _firePool;
    
    private readonly List<PoolObject> _flames = [];

    private void Awake() {
        var actorBody = GetComponentInChildren<ActorBody>(true);
        _collider = actorBody.GetComponent<Collider2D>();
    }

    private IEnumerator Start() {
        yield return new WaitUntil(() => PoolManager.Instance.allPools.Any(pool => pool._prefab.name == "Fire_FX_damage_Long jiechuan"));
        _firePool = PoolManager.Instance.allPools.FirstOrDefault(pool => pool._prefab.name == "Fire_FX_damage_Long jiechuan");
    }

    private void Update() {
        if (transform.position.y > groundY || _firePool == null) return;
        var colCenterX = _collider.bounds.center.x;
        var colBottomY = _collider.bounds.min.y;
        if (_flames.Count > 0) {
            var closestFlame = _flames.Aggregate(_flames[0], (closest, nextFlame) => {
                var nextDist = Mathf.Abs(nextFlame.transform.position.x - transform.position.x);
                var closeDist = Mathf.Abs(closest.transform.position.x - transform.position.x);
                return nextDist < closeDist ? nextFlame : closest;
            });
            var closestOffsetX = transform.position.x - closestFlame.transform.position.x;
            var closestDistance = Mathf.Abs(closestOffsetX);
            if (closestDistance > flameSpacing) {
                var spawnDirection = Mathf.Sign(closestOffsetX);
                var spawnX = closestFlame.transform.position.x + flameSpacing * spawnDirection;
                var condition = spawnDirection > 0 ? spawnX <= colCenterX : spawnX >= colCenterX;
                while (condition) {
                    var flameSpawnPos = new Vector2(spawnX, colBottomY);
                    SpawnFlame(flameSpawnPos);
                    spawnX += flameSpacing * spawnDirection;
                    condition = spawnDirection > 0 ? spawnX <= colCenterX : spawnX >= colCenterX;
                }
            }
        } else {
            var flameSpawnPos = new Vector2(colCenterX, colBottomY);
            SpawnFlame(flameSpawnPos);
        }
    }

    /// <summary>
    /// Spawn a Jiequan flame.
    /// </summary>
    /// <param name="position">The position to spawn the flame at.</param>
    public void SpawnFlame(Vector2 position) {
        var flame = _firePool.Borrow(position, Quaternion.identity);
        if (flame != null) {
            flame.OnReturnEvent.AddListener((obj) => {
                if (_flames.Contains(obj)) {
                    _flames.Remove(obj);
                }
            });
            _flames.Add(flame);
        }
    }

    /// <summary>
    /// Reset the flames list.
    /// </summary>
    public void ResetFlames() {
        _flames.Clear();
    }
}
