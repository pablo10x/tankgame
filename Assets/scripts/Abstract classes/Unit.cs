using System;
using Sirenix.OdinInspector;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
    void Heal(int amount);
    bool IsAlive();
}

public interface IAttackable
{
    void Attack(IDamageable target);
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

public enum UnitType
{
    unknown,
    Infantry,
    Vehicle,
    Aircraft,
    Structure,
    Turret,
    Hero,
}

public enum UnitFaction
{
    USA
}


public enum unitState {
    Idle,
    Moving,
    Guarding,
    Attacking,
    Capturing
}



[Serializable]
public abstract class Unit : MonoBehaviour, ISelectable {

    
    protected int MaxHealth;
    protected int Health;
    protected int BaseArmor;
    protected float MovementSpeed;

    protected unitState CurrentState;

    public bool isSelected = false;


    protected Unit CurrentlyAttacking = null;


    [BoxGroup("Unit Data")] public UnitData _UnitData;
    [BoxGroup("Unit Data")] public UnitType UnitType = UnitType.unknown;


    [BoxGroup("Unit Data")] public UnitFaction unitFaction;

    public void SetUpUnit()
    {
        MaxHealth = _UnitData.MaxHealth;
        Health = _UnitData.Health;
        BaseArmor = _UnitData.Armor;
        MovementSpeed = _UnitData.MovementSpeed;
    }

    public abstract void TakeDamage(int amount);
    public abstract void Heal(int amount);
    public abstract bool IsAlive();
    public abstract void Attack(IDamageable target);
    public abstract void OnDeath();

    public abstract void OnStateChanged(unitState oldstate, unitState newstate);
    
    
    
    public abstract void Move(Vector3 position);


    public void SetState(unitState state) {
        OnStateChanged(CurrentState,state);
    }
    
    public void OnSelect()
    {
//        Debug.Log($"{this.name} im selected");
    }

    public void OnDeselect()
    {
        Debug.Log($"{this.name} im DESELECTED");
    }
}