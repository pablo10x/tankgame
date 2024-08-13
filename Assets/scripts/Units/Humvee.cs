using EVP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Humvee : Unit, IDamageable, IDeathHandler, IAttackable {
    [SerializeField] private AiVehicleController AiVehicle;
    [SerializeField] private VehicleController vehicleController;

    public Humvee(float attackCooldown) {
        AttackCooldown = attackCooldown;
    }


    void Start() {
        SetUpUnit();
    }

    // Update is called once per frame
    void Update() {
    }

    public override void TakeDamage(int amount) {
        if (amount >= Health) {
            Health = 0;
            OnDeath();
        }

        Health -= amount;
        Debug.Log($"Taking damage::{amount} New unit health {Health}");
    }

    public override void Heal(int amount) {
    }

    public override bool IsAlive() {
        if (Health > 0)
            return true;
        return false;
    }


    public override void Attack(IDamageable target) {
        if (target.IsAlive()) {
        }
    }

    public float AttackCooldown { get; }

    public override void OnDeath() {
        EffectManager.Instance.Explode(vehicleController.cachedRigidbody);
    }

    public override void OnStateChanged(unitState oldstate, unitState newstate) {
       
    }

    public override void Move(Vector3 position) {
        if (IsAlive()) {
            AiVehicle.tryDriveTo(position);
        }
    }

   
}