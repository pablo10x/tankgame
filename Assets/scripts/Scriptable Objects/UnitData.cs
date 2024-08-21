using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[ CreateAssetMenu(menuName = "Unit/CreateNew", fileName = "unitData", order = 0) ]
public class UnitData : ScriptableObject {
    [ BoxGroup("Unit Data", CenterLabel = true, HideWhenChildrenAreInvisible = true) ]
    public int MaxHealth;

    [ BoxGroup("Unit Data", CenterLabel = true, HideWhenChildrenAreInvisible = true) ]
    public int Health;

    [ BoxGroup("Unit Data", CenterLabel = true, HideWhenChildrenAreInvisible = true) ]
    public int Armor;

    [ BoxGroup("Unit Data", CenterLabel = true, HideWhenChildrenAreInvisible = true) ]
    public float MovementSpeed;


    [ BoxGroup("Unit Data", CenterLabel = true, HideWhenChildrenAreInvisible = true) ]
    public float unitMaxRotationAngle;


    [ BoxGroup("Unit Data") ] public UnitType    UnitType = UnitType.unknown;
    [ BoxGroup("Unit Data") ] public UnitFaction unitFaction;
    [ BoxGroup("Unit Data") ] public bool        IsStationary = false;

    [ HideIf("IsStationary") ] [ BoxGroup("Movement") ]
    public float stoppingDistance = 0.1f;

    [ HideIf("IsStationary") ] [ BoxGroup("Movement") ]
    public float avoidanceRadius = 1f;

    [ HideIf("IsStationary") ] [ BoxGroup("Movement") ]
    public float avoidanceForce = 2f;

    [ HideIf("IsStationary") ] [ BoxGroup("Movement") ]
    public LayerMask obstacleLayer;

    [ HideIf("IsStationary") ] [ BoxGroup("Movement", AnimateVisibility = true, CenterLabel = true) ]
    [ HideIf("IsStationary") ]
    public float rotationSpeed = 5f;
}
