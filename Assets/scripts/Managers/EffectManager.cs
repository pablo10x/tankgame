using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EffectManager : Singleton<EffectManager>
{
    [FoldoutGroup("Vehicle Explosion")] public float     idleExplosionForce   = 1500f;
    [FoldoutGroup("Vehicle Explosion")] public float     movingExplosionForce = 1200f;
    [FoldoutGroup("Vehicle Explosion")] public float     explosionRadius      = 50f;
    [FoldoutGroup("Vehicle Explosion")] public float     maxRotationSpeed     = 180f;
    [FoldoutGroup("Vehicle Explosion")] public float     forwardDirection  = 5f;


    [BoxGroup("Testing")] public Unit Veh;

    public void Explode(Rigidbody targetRigidbody)
    {
        // Determine if the target is moving
        bool isMoving = targetRigidbody.velocity.magnitude > 0.1f; // Adjust threshold as needed

        // Calculate direction from explosion to target
        Vector3 direction = transform.position - transform.forward * forwardDirection;
        float   distance  = direction.magnitude;

        // Adjust force based on distance and vehicle state
        float forceMultiplier = Mathf.Clamp01(1 - distance / explosionRadius);
        float explosionForce  = isMoving ? movingExplosionForce : idleExplosionForce;
        float force           = explosionForce * forceMultiplier;

        // Apply impulse force
        targetRigidbody.AddForce(direction.normalized * force, ForceMode.Impulse);

        // Add random torque for flipping
        float   torqueMultiplier = 10f; // Adjust as needed
        Vector3 torqueAxis       = Random.onUnitSphere;
        targetRigidbody.AddTorque(torqueAxis * explosionForce * torqueMultiplier, ForceMode.Impulse);
    }


    public void GiveUnitDamage(Unit _unit, int damage) => _unit.TakeDamage(damage);


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
           Veh.OnDeath();
        }
    }
}
