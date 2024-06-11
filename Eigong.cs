using UnityEngine;

namespace InnerEigong;

/// <summary>
/// Modifies the behavior of the Eigong boss.
/// </summary>
internal class Eigong : MonoBehaviour {
    private Health _health;
    private StealthGameMonster _monsterBase;

    private void Awake() {
        _monsterBase = GetComponentInChildren<StealthGameMonster>();

        _health = _monsterBase.health;

        gameObject.AddComponent<FireTrail>();

        var shield = GetComponentInChildren<MonsterShield>();
        shield.transform.localScale = Vector3.one;
        shield.gameObject.SetActive(true);
        shield.Reset();
    }
}
