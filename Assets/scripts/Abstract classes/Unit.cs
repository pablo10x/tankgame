using System;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
    void Heal(int       amount);
    bool IsAlive();
}

public interface IAttackable
{
    void  Attack(IDamageable target);
    float AttackCooldown { get; }
}

public interface IDeathHandler
{
    void OnDeath();
}
public interface ISelectable
{
    void OnSelect();
    void OnDeselect();
}


//unit types

public enum UnitType {
    Infantry,
    Vehicle,
    Aircraft,
    Structure,
    Turret,
    Hero,
}


[Serializable]
public abstract class Unit : MonoBehaviour {
    
    protected int MaxHealth;
    protected int Health;
    protected int baseArmor;
    protected float MovementSpeed;


    [BoxGroup("Unit Data")] public UnitData _UnitData;

    public void setUpUnit()
    {
        MaxHealth     = _UnitData.MaxHealth;
        Health    = _UnitData.Health;
        baseArmor = _UnitData.Armor;
        MovementSpeed  = _UnitData.MovementSpeed;

        Debug.Log($"Base: {Health}");
    }

    public abstract void TakeDamage(int amount);
    public abstract void Heal(int       amount);
    public abstract bool IsAlive();
    public abstract void Attack(IDamageable target);
    public abstract void OnDeath();

   
}

