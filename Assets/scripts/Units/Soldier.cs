using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Soldier:Unit, IDamageable, IAttackable, IDeathHandler {

    [FoldoutGroup("Refs")]
    [SerializeField]
    private NavMeshAgent navigator;

    private void Start () {
    }
    

    private void LateUpdate () {
    }

    public override void TakeDamage (int amount) {
        throw new System.NotImplementedException();
    }

    public override void Heal (int amount) {
        Health += amount;
    }

    public override bool IsAlive () {
        throw new System.NotImplementedException();
    }

    public override void Attack (IDamageable target) {
    }

    public float AttackCooldown { get; }

    public override void OnDeath () {

    }

    public override void OnStateChanged(unitState oldstate, unitState newstate) {
        
    }

    public override void Move (Vector3 position) {

    }
}