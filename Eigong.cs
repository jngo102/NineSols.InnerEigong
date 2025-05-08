using System.Reflection;
using Cysharp.Threading.Tasks;
using MonsterLove.StateMachine;
using UnityEngine;
using States = MonsterBase.States;

namespace InnerEigong;

/// <summary>
/// Modifies the behavior of the Eigong boss.
/// </summary>
internal class Eigong : MonoBehaviour {
    private StealthGameMonster _monster;

    private void Awake() {
        TryGetComponent(out _monster);
        _monster.OverrideWanderingIdleTime(0); 
        _monster.StartingPhaseIndex = 1;

        var monsterCore = _monster.monsterCore;

        // var phaseSequences = monsterCore.attackSequenceMoodule.GetType().GetField("SequenceForDifferentPhase", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(monsterCore.attackSequenceMoodule) as MonsterStateSequenceWeight[];
        // foreach (var seq in phaseSequences) {
        //     Plugin.Instance.Log($"Phase Sequence:\t{seq.name}");
        //     foreach (var seqWgt in seq.setting.stateWeightList) {
        //         Plugin.Instance.Log($"Sequence Weight:\t{seqWgt.weight}");
        //         var attSeq = seqWgt.option.AttackSequence;
        //         for (var i = 0; i < attSeq.Count; i++) {
        //             Plugin.Instance.Log($"\tAttack {attSeq[i].name} ({i}):");
        //             foreach (var attWgt in attSeq[i].setting.stateWeightList) {
        //                 Plugin.Instance.Log($"\t\tAttack Sequence:\tWeight:\t{attWgt.weight},\tState: {attWgt.option.name}");
        //             }
        //         }
        //     }
        // }

        //monsterCore.AnimationSpeed = 1.25f

        // var anim = _monster.animator;
        // var clips = anim.runtimeAnimatorController.animationClips;
        // var tex = Texture2D.whiteTexture;
        // var spriteCurve = AnimationCurve.Constant(0, 1, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f)));
        //
        // foreach (var clip in clips) {
        //     clip.SetCurve("MonsterCore/Animator(Proxy)/Animator", typeof(SpriteRenderer), nameof(SpriteRenderer.sprite), spriteCurve);
        // }
        // var events =
        //     clips[0].GetEventsInternal(); //;SetCurve("MonsterCore/Animator(Proxy)/Animator", typeof(SpriteRenderer), "sprite", );
        // for (var layer = 0; layer < anim.layerCount; layer++)
        //     Plugin.Instance.Log($"Layer {layer}: {anim.GetLayerName(layer)}");
        // foreach (var param in anim.parameters) Plugin.Instance.Log($"Param: {param.name}\t{param.m_Type}");
        //
        // var states = _monster.GetComponentsInChildren<BossGeneralState>(true);
        // foreach (var state in states) {
        //     Plugin.Instance.Log($"Setting state {state.name} active");
        //     state.gameObject.SetActive(true);
        //     state.OverideAnimationSpeed = true;
        //     state.AnimationSpeed *= 1.2f;
        // }

        // var stat = _monster.monsterStat;
        // var statType = stat.GetType();
        // var attackVal = statType.GetField("BaseAttackValue", BindingFlags.Instance | BindingFlags.NonPublic);
        // attackVal?.SetValue(stat, (float)attackVal.GetValue(stat) * 1.25f);
        // var healthVal = statType.GetField("BaseHealthValue", BindingFlags.Instance | BindingFlags.NonPublic);
        // healthVal?.SetValue(stat, (float)healthVal.GetValue(stat) * 1.25f);

        // var shield = GetComponentInChildren<MonsterShield>(true);
        // shield.transform.localScale = Vector3.one;
        // shield.gameObject.SetActive(true);
        // shield.Reset();

        // var animEvents = GetComponentInChildren<AnimationEvents>();
        // animEvents.animationEvent.AddListener(@event => Plugin.Instance.Log("ANIM EVENT: " + @event));
    }

    private StateMachine<States> _stateMachine;

    private async void Start() {
        await UniTask.WaitUntil(() => _monster.fsm != null);

        _stateMachine = _monster.fsm;
        var runner = _stateMachine.runner;

        var slowStartFullCombo = _monster.GetState(States.Attack1);
        var teleportToBigWhiteFlash = _monster.GetState(States.Attack2);
        var issenSlash = _monster.GetState(States.Attack3);
        var uppercutToWhiteSlash = _monster.GetState(States.Attack4);
        var teleportBack = _monster.GetState(States.Attack5);
        var slowStartTrailingCombo = _monster.GetState(States.Attack6);
        var teleportToSlowStartTrailingCombo = _monster.GetState(States.Attack7);
        var sheathToRedWhiteWhite = _monster.GetState(States.Attack8);
        var guardToSlowStartOrTriplePoke= _monster.GetState(States.Attack9);
        var faceAndChargeTalisman = _monster.GetState(States.Attack10);
        var redWhiteWhite = _monster.GetState(States.Attack11);
        var uppercutToFirePillar = _monster.GetState(States.Attack12);
        var triplePoke = _monster.GetState(States.Attack13);
        var chargeBigBall = _monster.GetState(States.Attack14);
        var chargeToTurnTalisman = _monster.GetState(States.Attack15);
        var faceTalisman = _monster.GetState(States.Attack16);
        var overheadToIssenSlashOrTalisman = _monster.GetState(States.Attack17);
        var farTeleportToChargeTalisman= _monster.GetState(States.Attack18);
        var teleportToBigRedCut = _monster.GetState(States.Attack19);
        var bigWhiteFlash = _monster.GetState(States.Attack20);
        
        foreach (var groupSequence in GetComponentsInChildren<MonsterStateGroupSequence>(true)) {
            var attackSequences = groupSequence.AttackSequence;
            foreach (var attackSequence in attackSequences) {
                attackSequence.setting.queue = [slowStartFullCombo, issenSlash, redWhiteWhite];   
            }
            groupSequence.AttackSequence = attackSequences;
        }

        var attackSequencer = _monster.monsterCore.attackSequenceMoodule;
        var phaseSequencesField = attackSequencer.GetType()
            .GetField("SequenceForDifferentPhase", BindingFlags.Instance | BindingFlags.NonPublic);
        if (phaseSequencesField != null) {
            var phaseSequences = (MonsterStateSequenceWeight[])phaseSequencesField.GetValue(attackSequencer);
            foreach (var phaseSequence in phaseSequences) {
                foreach (var groupSequence in phaseSequence.setting.queue) {
                    var attackSequences = groupSequence.AttackSequence;
                    foreach (var attackSequence in attackSequences) {
                        var setting = attackSequence.setting;
                        setting.queue = [slowStartFullCombo, issenSlash, redWhiteWhite];
                        attackSequence.setting = setting;
                    }

                    groupSequence.AttackSequence = attackSequences;
                }
            }    
            phaseSequencesField.SetValue(attackSequencer, phaseSequences);
        }
        
        
        ModifyStates();
    }

    private void ModifyStates() {
        foreach (var state in GetComponentsInChildren<BossGeneralState>(true)) {
        }
    }
}