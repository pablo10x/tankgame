using EVP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class Humvee : Unit, IDamageable, IDeathHandler, IAttackable, ISelectable {
    [SerializeField] private AiVehicleController _AivehicleController;
    [SerializeField] private VehicleController   vehicleController;


    void Start()
    {
        setUpUnit();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void TakeDamage(int amount)
    {
        if (amount >= Health) {
            Health = 0;
            OnDeath();
        }

        Health -= amount;
        Debug.Log($"Taking damage::{amount} New unit health {Health}");
    }

    public override void Heal(int amount)
    {
    }

    public override bool IsAlive()
    {
        if (Health > 0) return true;
        return false;
    }

    public override void Attack(IDamageable target)
    {
    }

    public float AttackCooldown { get; }

    public override void OnDeath()
    {
        EffectManager.Instance.Explode(vehicleController.cachedRigidbody);
    }

    public void OnSelect()
    {
    }

    public void OnDeselect()
    {
    }
}