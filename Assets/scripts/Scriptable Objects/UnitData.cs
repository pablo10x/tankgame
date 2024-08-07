using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Unit/CreateNew", fileName = "unitData", order = 0)]
public class UnitData : ScriptableObject
{
    public int   MaxHealth;
    public int   Health;
    public int   Armor;
    public float MovementSpeed;
}
