using System;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : Unit, IDamageable, IAttackable, IDeathHandler, IMoveable {
    public Soldier(float attackDistance, float attackCooldown) {
        AttackCooldown = attackCooldown;
    }


    protected override void Start() {
        base.Start();
    }

    public override void TakeDamage(int amount) {
        Health -= amount;
        if (Health <= 0) {
            Health = 0;
            OnDeath();
        }
    }

    public override void Heal(int amount) {
        Health += amount;
        if (Health > MaxHealth) {
            Health = MaxHealth;
        }
    }


    public float AttackPerHit => 5f;

    public override void Attack(IDamageable target) { }

    public float AttackCooldown {
        get;
        set;
    }

    public bool isAttacking {
        get;
        set;
    }

    public override void OnDeath() {
        if (UnitsManager.Instance.selectedUnits.Contains(this))
            UnitsManager.Instance.selectedUnits.Remove(this);
        if (UnitsManager.Instance.RegistredUnits.ContainsValue(this))
            UnitsManager.Instance.RegistredUnits.Remove(_UnitCollider);

        Destroy(gameObject, 8);
    }

    public override void OnStateChanged(unitState oldstate, unitState newstate) { }

    public void Move(Vector3 position) {
        SetDestination(position);
    }

    public void Stop() {
        throw new NotImplementedException();
    }
}
